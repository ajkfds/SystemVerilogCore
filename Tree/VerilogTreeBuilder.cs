using System;
using System.Collections.Generic;
using System.Linq;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;
using SystemVerilogCore.Verilog.BuildingBlocks;
using SystemVerilogCore.Verilog.Items;

namespace SystemVerilogCore.Tree
{
    /// <summary>
    /// Decides what the instance tree contains: which children a file has, which
    /// instantiation becomes which node, and when an existing node can be reused.
    ///
    /// <para>
    /// <b>Why this lives here.</b> These are rules about the parse result, not
    /// about the UI, but they lived in the editor plugin
    /// (<c>VerilogCommon.Updater</c>) and wrote straight into the editor's
    /// <c>Item.ItemList</c>. A host without that item tree therefore had to
    /// re-implement them, and the two implementations drifted. The builder
    /// produces <see cref="IVerilogTreeNode"/>s through the host's
    /// <see cref="IVerilogTreeNodeFactory"/>, so the editor and the language
    /// server walk one rule set.
    /// </para>
    ///
    /// <para>
    /// The rules, in the order they are applied (they mirror the editor's
    /// NavigatePanel, which is what the VS Code tree has to match):
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>a file with one building block lists that block's instantiations
    /// directly; a file with several lists one node per building block
    /// (the editor's <c>addSubItemsSingleBuldingBlock</c> /
    /// <c>addSubItemsMultiBuildingBlock</c>);</item>
    /// <item>an instance (module / interface) lists the instantiations of the
    /// building block it was parsed as, which is how the tree nests;</item>
    /// <item>a package is a leaf;</item>
    /// <item>include headers and imported packages are children of the file that
    /// mentions them, before the instantiations;</item>
    /// <item>an instance array (<c>u[1:0]</c>) becomes one node per element;</item>
    /// <item>an existing node is reused when its instance key (module name plus
    /// parameter overrides) matches, so re-running the builder keeps the
    /// editor's selection and expansion state.</item>
    /// </list>
    /// </summary>
    public static class VerilogTreeBuilder
    {
        /// <summary>
        /// Builds the children of <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent">The node being expanded.</param>
        /// <param name="factory">Creates / finds the host's nodes.</param>
        /// <param name="buildingBlockName">
        /// For an instance, the building block it was parsed as. Null for a file,
        /// which lists its own building blocks.
        /// </param>
        /// <returns>The children, in display order.</returns>
        public static IReadOnlyList<IVerilogTreeNode> BuildChildren(
            IVerilogTreeNode parent,
            IVerilogTreeNodeFactory factory,
            string? buildingBlockName)
        {
            List<IVerilogTreeNode> children = new();
            if (parent == null || factory == null) return children;

            ParsedDocument? parsed = parent.ParsedResult as ParsedDocument;
            if (parsed?.Root == null) return children;

            // Include headers and imported packages come first: the editor's
            // NavigatePanel shows them above the instantiations, and the VS Code
            // tree has to look the same.
            foreach (IVerilogIncludeFile include in parsed.IncludeFiles.Values)
            {
                IVerilogTreeNode? header = factory.CreateHeader(parent, include);
                if (header != null) children.Add(header);
            }

            foreach (string packageName in parsed.ImportedPackages)
            {
                IVerilogTreeNode? package = factory.CreateImportedPackage(parent, packageName);
                if (package != null) children.Add(package);
            }

            switch (parent.Kind)
            {
                case VerilogTreeNodeKind.File:
                    // A file lists the instantiations of its building block(s).
                    // With one block they are the file's direct children (so a
                    // single-module file does not gain a redundant level); with
                    // several, each block becomes a node of its own and its
                    // instantiations nest under that.
                    if (parsed.Root.BuildingBlocks.Count == 1)
                    {
                        BuildingBlock only = parsed.Root.BuildingBlocks.Values.First();
                        AppendInstantiations(children, parent, factory, only);
                    }
                    else
                    {
                        foreach (BuildingBlock block in parsed.Root.BuildingBlocks.Values)
                        {
                            AppendBuildingBlockNode(children, parent, factory, block);
                        }
                    }
                    break;

                case VerilogTreeNodeKind.ModuleInstance:
                case VerilogTreeNodeKind.InterfaceInstance:
                    // The instance lists what its own building block instantiates,
                    // which is what nests the tree.
                    if (!string.IsNullOrEmpty(buildingBlockName)
                        && parsed.Root.BuildingBlocks.TryGetValue(buildingBlockName, out BuildingBlock? ownBlock))
                    {
                        AppendInstantiations(children, parent, factory, ownBlock);
                    }
                    break;

                case VerilogTreeNodeKind.ImportedPackage:
                case VerilogTreeNodeKind.Header:
                    // Leaves in the navigate tree: a package's own instantiations
                    // are not shown, and an include is a file reference.
                    break;
            }

            return children;
        }

        /// <summary>
        /// One node per building block of a multi-block file, with that block's
        /// instantiations nested under it.
        /// </summary>
        private static void AppendBuildingBlockNode(
            List<IVerilogTreeNode> children,
            IVerilogTreeNode parent,
            IVerilogTreeNodeFactory factory,
            BuildingBlock block)
        {
            IVerilogTreeNode? existing = factory.FindChild(parent, block.Name);
            if (existing != null && existing.InstanceKey == null)
            {
                // Same building block as before: reuse the node so the editor
                // keeps its selection / expansion state.
                children.Add(existing);
                return;
            }

            // The node stands for the block itself (the editor models it as a
            // module / interface instance named after the block), so the host
            // builds it.
            IVerilogTreeNode? node = factory.CreateBuildingBlockNode(parent, block);
            if (node == null) return;
            children.Add(node);
        }

        /// <summary>
        /// The nodes for the instantiations of <paramref name="block"/>.
        /// </summary>
        private static void AppendInstantiations(
            List<IVerilogTreeNode> children,
            IVerilogTreeNode parent,
            IVerilogTreeNodeFactory factory,
            BuildingBlock block)
        {
            foreach (IBuildingBlockInstantiation instantiation in block.GetBuildingBlockInstantiations())
            {
                switch (instantiation)
                {
                    case ModuleInstantiation moduleInstantiation:
                        AppendModuleInstantiation(children, parent, factory, moduleInstantiation);
                        break;

                    case Verilog.Items.InterfaceInstance interfaceInstantiation:
                        AppendInterfaceInstantiation(children, parent, factory, interfaceInstantiation);
                        break;
                }
            }
        }

        private static void AppendModuleInstantiation(
            List<IVerilogTreeNode> children,
            IVerilogTreeNode parent,
            IVerilogTreeNodeFactory factory,
            ModuleInstantiation instantiation)
        {
            // Reuse an existing node when it stands for the SAME instance: the
            // key carries the module name and the parameter overrides, so
            // "child #(.WIDTH(4)) u(...)" and "child u(...)" are different nodes.
            if (IsSameInstance(parent, factory, instantiation))
            {
                IVerilogTreeNode? existing = factory.FindChild(parent, instantiation.Name);
                if (existing != null)
                {
                    children.Add(existing);
                    return;
                }
            }

            // Instance array: one node per element.
            if (instantiation.InstanceRange != null)
            {
                IReadOnlyList<IVerilogTreeNode>? array = factory.CreateModuleInstanceArray(parent, instantiation);
                if (array != null)
                {
                    foreach (IVerilogTreeNode element in array)
                    {
                        if (element == null) continue;
                        if (children.Any(x => string.Equals(x.Name, element.Name, StringComparison.Ordinal))) continue;
                        children.Add(element);
                    }
                    return;
                }
            }

            IVerilogTreeNode? node = factory.CreateModuleInstance(parent, instantiation);
            if (node == null) return;
            if (children.Any(x => string.Equals(x.Name, node.Name, StringComparison.Ordinal))) return;
            children.Add(node);
        }

        private static void AppendInterfaceInstantiation(
            List<IVerilogTreeNode> children,
            IVerilogTreeNode parent,
            IVerilogTreeNodeFactory factory,
            Verilog.Items.InterfaceInstance instantiation)
        {
            IVerilogTreeNode? existing = factory.FindChild(parent, instantiation.Name);
            if (existing != null
                && string.Equals(existing.InstanceKey, instantiation.OverrideParameterID, StringComparison.Ordinal))
            {
                children.Add(existing);
                return;
            }

            IVerilogTreeNode? node = factory.CreateInterfaceInstance(parent, instantiation);
            if (node == null) return;
            if (children.Any(x => string.Equals(x.Name, node.Name, StringComparison.Ordinal))) return;
            children.Add(node);
        }

        /// <summary>
        /// True when the existing child under the instantiation's name stands for
        /// the same instance (same module, same parameter overrides).
        /// </summary>
        private static bool IsSameInstance(
            IVerilogTreeNode parent,
            IVerilogTreeNodeFactory factory,
            ModuleInstantiation instantiation)
        {
            IVerilogTreeNode? existing = factory.FindChild(parent, instantiation.Name);
            if (existing == null) return false;

            string? expectedKey = InstanceKey(instantiation);
            if (expectedKey == null) return false;
            return string.Equals(existing.InstanceKey, expectedKey, StringComparison.Ordinal);
        }

        /// <summary>
        /// The parse-cache key an instantiation resolves under (module name plus
        /// parameter overrides), or null when the instantiated module's file
        /// cannot be resolved.
        /// </summary>
        /// <remarks>
        /// Same key the editor's instance uses
        /// (<c>VerilogModuleInstance.Key</c>), so a re-run of the builder matches
        /// the node it made last time.
        /// </remarks>
        private static string? InstanceKey(ModuleInstantiation instantiation)
        {
            IVerilogSourceFile? sourceFile = instantiation.GetInstancedBuildingBlockProject() is { } project
                ? (project.Settings as Verilog.IVerilogParserSettings)?.DefinitionNameSpace
                    .GetFile(instantiation.SourceName) as IVerilogSourceFile
                : null;
            if (sourceFile == null) return null;
            return ParsedDocument.KeyGenerator(
                sourceFile, instantiation.SourceName, instantiation.ParameterOverrides);
        }
    }
}
