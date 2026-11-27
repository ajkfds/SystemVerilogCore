using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Parsing;
using SystemVerilogCore.Verilog;

namespace SystemVerilogCore.Tree
{
    /// <summary>What a node in the instance tree represents.</summary>
    public enum VerilogTreeNodeKind
    {
        /// <summary>A source file (its own parse result).</summary>
        File,

        /// <summary>An instantiated module.</summary>
        ModuleInstance,

        /// <summary>An instantiated interface.</summary>
        InterfaceInstance,

        /// <summary>An <c>import</c>ed package.</summary>
        ImportedPackage,

        /// <summary>A file pulled in by <c>include</c>.</summary>
        Header,
    }

    /// <summary>
    /// One node of the instance tree (the desktop editor's NavigatePanel, the
    /// VS Code RTL Hierarchy).
    ///
    /// <para>
    /// <b>Why this exists.</b> The tree's <b>content</b> - which children a file
    /// has, which instantiation becomes which node, when an existing node can be
    /// reused - is a rule about the parse result, not about the UI. It lived in
    /// the editor plugin (<c>VerilogCommon.Updater</c>) and wrote straight into
    /// the editor's <c>Item.ItemList</c>, so a host without that item tree had to
    /// re-implement the rules or reference the editor stack.
    /// </para>
    ///
    /// <para>
    /// The rules now live in <see cref="VerilogTreeBuilder"/>, which produces
    /// these nodes. A host supplies a <see cref="IVerilogTreeNodeFactory"/> so the
    /// nodes can be the host's own objects (the editor's
    /// <c>VerilogModuleInstance</c> is also a file and an item-tree entry); the
    /// builder only decides <b>what</b> the tree contains.
    /// </para>
    /// </summary>
    public interface IVerilogTreeNode
    {
        /// <summary>What this node represents.</summary>
        VerilogTreeNodeKind Kind { get; }

        /// <summary>Display name (the instance name, the package name, the file name).</summary>
        string Name { get; }

        /// <summary>Project-relative path of the file this node stands for.</summary>
        string RelativePath { get; }

        /// <summary>
        /// The parse-cache key of an instance (module name plus parameter
        /// overrides), or null for a node that is not an instance.
        ///
        /// <para>
        /// The builder compares this with the key it would generate for the
        /// instantiation to decide whether an existing node can be reused: an
        /// instance written as <c>child #(.WIDTH(4)) u(...)</c> is a different
        /// node from <c>child u2(...)</c>, and reusing by name alone would show
        /// the wrong parameter overrides.
        /// </para>
        /// </summary>
        string? InstanceKey { get; }

        /// <summary>The node's parse result, or null when it has not been parsed.</summary>
        IParsedDocument? ParsedResult { get; }

        /// <summary>This node's children, in display order.</summary>
        IReadOnlyList<IVerilogTreeNode> Children { get; }
    }

    /// <summary>
    /// Creates the host's tree nodes for the builder.
    ///
    /// <para>
    /// The builder decides which nodes exist; the host decides what they are. The
    /// editor's nodes are also files and item-tree entries
    /// (<c>VerilogModuleInstance</c>, <c>InterfaceInstance</c>,
    /// <c>ImportedPackage</c>, <c>VerilogHeaderInstance</c>), which the UI-free
    /// assembly cannot name, so it asks for them.
    /// </para>
    /// </summary>
    public interface IVerilogTreeNodeFactory
    {
        /// <summary>
        /// The child currently registered under <paramref name="key"/>, or null.
        /// The builder uses it to reuse a node instead of re-creating it (a
        /// re-created node would lose the editor's selection and expansion state).
        /// </summary>
        IVerilogTreeNode? FindChild(IVerilogTreeNode parent, string key);

        /// <summary>Creates the node for a module instantiation.</summary>
        IVerilogTreeNode? CreateModuleInstance(
            IVerilogTreeNode parent,
            Verilog.Items.ModuleInstantiation instantiation);

        /// <summary>
        /// Creates the nodes for an instance array
        /// (<c>child u[1:0](...)</c>), one per element, or null when the range
        /// cannot be evaluated.
        /// </summary>
        IReadOnlyList<IVerilogTreeNode>? CreateModuleInstanceArray(
            IVerilogTreeNode parent,
            Verilog.Items.ModuleInstantiation instantiation);

        /// <summary>Creates the node for an interface instantiation.</summary>
        IVerilogTreeNode? CreateInterfaceInstance(
            IVerilogTreeNode parent,
            Verilog.Items.InterfaceInstance instantiation);

        /// <summary>
        /// Creates the node that stands for a building block listed directly by
        /// its own file (a multi-block file shows one node per block).
        /// </summary>
        /// <remarks>
        /// The editor models that node as a module / interface instance whose
        /// name is the block's name; a host without an item tree can build any
        /// node it likes. Returning null skips the block.
        /// </remarks>
        IVerilogTreeNode? CreateBuildingBlockNode(
            IVerilogTreeNode parent,
            Verilog.BuildingBlocks.BuildingBlock block);

        /// <summary>Creates the node for an imported package.</summary>
        IVerilogTreeNode? CreateImportedPackage(IVerilogTreeNode parent, string packageName);

        /// <summary>Creates the node for an include.</summary>
        IVerilogTreeNode? CreateHeader(IVerilogTreeNode parent, IVerilogIncludeFile include);
    }
}
