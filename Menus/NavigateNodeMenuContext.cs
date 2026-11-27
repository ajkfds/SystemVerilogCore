using System;
using System.Collections.Generic;
using SystemVerilogCore.Documents;

namespace SystemVerilogCore.Menus
{
    /// <summary>
    /// The node a navigate-node menu is being built for, described the same way
    /// by every host.
    ///
    /// <para>
    /// The desktop editor knows the node as a <c>NavigatePanelNode</c> and VS Code
    /// knows it as a tree item, but every row only needs the same handful of
    /// facts: which kind of node it is, which file it represents, and the paths
    /// the file operations act on. Keeping that in one plain class is what lets
    /// <see cref="NavigateNodeMenuBuilder"/> serve both hosts without knowing
    /// either UI.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.NavigateNodeMenuContext</c>), so the language
    /// server had to reference the editor stack to build a node menu. The file is
    /// the UI-free <see cref="IVerilogSourceFile"/> contract now.
    /// </para>
    /// </summary>
    public sealed class NavigateNodeMenuContext
    {
        /// <summary>Kind of node the menu is for.</summary>
        public HierarchyMenuCatalog.AppliesTo Kind { get; init; } = HierarchyMenuCatalog.AppliesTo.None;

        /// <summary>
        /// The file the node belongs to. For an instance this is the instance's
        /// own parse result (it carries the parameter overrides written at the
        /// instantiation), which is what the simulation setup needs.
        /// </summary>
        public IVerilogSourceFile? File { get; init; }

        /// <summary>Text shown in the tree for the node.</summary>
        public string? NodeLabel { get; init; }

        /// <summary>Readable instance path ("u_mid.u_leaf"); null for files.</summary>
        public string? InstancePath { get; init; }

        /// <summary>Workspace-relative path ("src/rtl/top.sv"), used by Copy Path.</summary>
        public string? WorkspaceRelativePath { get; init; }

        /// <summary>Absolute path of the node's file.</summary>
        public string? FsPath { get; init; }

        /// <summary>
        /// Absolute directory the node lives in: a folder node is its own
        /// directory, a file / instance uses the directory of its file. This is
        /// where the Add operations create the new item.
        /// </summary>
        public string? DirPath { get; init; }

        /// <summary>
        /// Produces the body of the "simulation setup" row, or null when this
        /// host cannot collect it.
        ///
        /// <para>
        /// The row lists the files the simulator would compile, which is collected
        /// by walking the host's item tree. That tree belongs to the host (the
        /// editor's project / the language server's workspace), so the collection
        /// is a host operation: this contract only carries the callback, and the
        /// row reports itself as unavailable when no host supplied one.
        /// </para>
        /// </summary>
        public Func<NavigateNodeMenuContext, string?>? SimulationSetupText { get; init; }

        /// <summary>
        /// Maps the VS Code node kind string ("folder" / "file" / "instance")
        /// onto the catalog flags. Unknown kinds yield
        /// <see cref="HierarchyMenuCatalog.AppliesTo.None"/>, which makes the
        /// menu empty rather than wrong.
        /// </summary>
        public static HierarchyMenuCatalog.AppliesTo KindFromName(string? nodeKind)
        {
            switch (nodeKind)
            {
                case "project": return HierarchyMenuCatalog.AppliesTo.Project;
                case "folder": return HierarchyMenuCatalog.AppliesTo.Folder;
                case "file": return HierarchyMenuCatalog.AppliesTo.File;
                case "instance": return HierarchyMenuCatalog.AppliesTo.Instance;
                case "header": return HierarchyMenuCatalog.AppliesTo.Header;
                case "package": return HierarchyMenuCatalog.AppliesTo.Package;
                default: return HierarchyMenuCatalog.AppliesTo.None;
            }
        }
    }
}
