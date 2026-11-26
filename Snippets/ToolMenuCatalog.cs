using System.Collections.Generic;

namespace SystemVerilogCore.Snippets
{
    /// <summary>
    /// The tool menu (Shift+Space in the desktop editor) as data.
    ///
    /// <para>
    /// The editor lists its tools and the VS Code extension has to list the same
    /// tools in a QuickPick. If each side kept its own copy, adding a tool would
    /// mean editing two places and the two menus would drift. This catalog is the
    /// one list:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>the editor iterates it to build its <c>ToolItem</c> list,</item>
    /// <item>the LSP host serves it through <c>rtl/toolMenu</c> so the extension
    /// can render the same menu (see <see cref="ToolMenuBuilder"/>).</item>
    /// </list>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.ToolMenuCatalog</c>), so the language server
    /// had to reference the editor stack to know which tools exist. The list is
    /// plain data, so nothing needs a host type.
    /// </para>
    /// </summary>
    public static class ToolMenuCatalog
    {
        /// <summary>
        /// Stable tool identifiers. They cross the LSP boundary
        /// (<c>rtl/toolMenu</c> answers with these), so they must not change.
        /// </summary>
        public static class ToolId
        {
            /// <summary>An always block with the clock / reset from the ports.</summary>
            public const string AlwaysFF = "alwaysFF";

            /// <summary>Open the port list of the instantiation at the caret.</summary>
            public const string AutoConnect = "autoConnect";

            /// <summary>Reformat the instantiation at the caret.</summary>
            public const string AutoFormat = "autoFormat";

            /// <summary>Insert a module instance.</summary>
            public const string ModuleInstance = "moduleInstance";

            /// <summary>Fill the port connections of the instantiation at the caret.</summary>
            public const string PortConnectionCreate = "portConnectionCreate";

            /// <summary>Invert the direction / suffix of the selected ports.</summary>
            public const string PortInvert = "portInvert";

            /// <summary>Generate a modport from the enclosing interface.</summary>
            public const string ModPort = "modport";

            /// <summary>Trace where a signal is connected (desktop only).</summary>
            public const string ConnectionCheck = "connectionCheck";

            /// <summary>Upper-case the selection.</summary>
            public const string ToUpper = "toUpper";

            /// <summary>Lower-case the selection.</summary>
            public const string ToLower = "toLower";
        }

        /// <summary>One row of the tool menu.</summary>
        public sealed class Entry
        {
            /// <summary>Stable id (one of <see cref="ToolId"/>).</summary>
            public string Id { get; init; } = string.Empty;

            /// <summary>Menu label.</summary>
            public string Label { get; init; } = string.Empty;

            /// <summary>One line explaining what the tool does.</summary>
            public string Detail { get; init; } = string.Empty;

            /// <summary>
            /// <c>true</c> for the tools that work on the current selection
            /// (port inversion, case transforms). Informational: the host uses
            /// it to explain why a row is disabled when nothing is selected.
            /// </summary>
            public bool AppliesToSelection { get; init; }

            /// <summary>
            /// <c>true</c> for the selection transforms the editor appends on
            /// its own, so <see cref="EditorTools"/> must not list them a second
            /// time.
            ///
            /// <para>
            /// Deliberately separate from <see cref="AppliesToSelection"/>:
            /// port inversion also needs a selection but the editor does NOT
            /// append it (it comes from the file's tool list), so filtering on
            /// <see cref="AppliesToSelection"/> would drop it from the editor's
            /// Shift+Space menu.
            /// </para>
            /// </summary>
            public bool AppendedByEditor { get; init; }

            /// <summary>
            /// <c>true</c> when the tool needs a window the desktop editor owns
            /// (a tree view / dialog), so an LSP host can only report it as
            /// unavailable. See <see cref="ToolMenuBuilder"/>.
            /// </summary>
            public bool DesktopOnly { get; init; }
        }

        private static readonly Entry[] entries =
        {
            new Entry
            {
                Id = ToolId.AlwaysFF,
                Label = "alwaysFF",
                Detail = "always block with the clock / reset taken from the ports",
            },
            new Entry
            {
                Id = ToolId.AutoConnect,
                Label = "Auto Connect Ports",
                Detail = "one row per port with the signal the editor would connect",
            },
            new Entry
            {
                Id = ToolId.AutoFormat,
                Label = "AutoFormat Instance",
                Detail = "reformat the module instantiation at the caret",
            },
            new Entry
            {
                Id = ToolId.ModuleInstance,
                Label = "Insert Module Instance",
                Detail = "instantiate a module with its parameters and port connections",
            },
            new Entry
            {
                Id = ToolId.PortConnectionCreate,
                Label = "Fill Port Connections",
                Detail = "rewrite the instantiation at the caret with named connections",
            },
            new Entry
            {
                Id = ToolId.PortInvert,
                Label = "Invert Ports",
                Detail = "flip input/output and the _I/_O suffix of the selection",
                AppliesToSelection = true,
            },
            new Entry
            {
                Id = ToolId.ModPort,
                Label = "Generate Modport",
                Detail = "modport from the signals of the enclosing interface",
            },
            new Entry
            {
                Id = ToolId.ConnectionCheck,
                Label = "Connection Check",
                Detail = "trace where a signal is connected",
                DesktopOnly = true,
            },
            new Entry
            {
                Id = ToolId.ToUpper,
                Label = "To Upper Case",
                Detail = "transform the selection",
                AppliesToSelection = true,
                AppendedByEditor = true,
            },
            new Entry
            {
                Id = ToolId.ToLower,
                Label = "To Lower Case",
                Detail = "transform the selection",
                AppliesToSelection = true,
                AppendedByEditor = true,
            },
        };

        /// <summary>Every tool, in menu order.</summary>
        public static IReadOnlyList<Entry> All { get; } = entries;

        /// <summary>
        /// The tools the editor puts in its own list. The case transforms are
        /// excluded because the editor appends them after asking the file for its
        /// tools (listing them here as well would show each of them twice).
        /// </summary>
        public static IReadOnlyList<Entry> EditorTools { get; } = BuildEditorTools();

        private static IReadOnlyList<Entry> BuildEditorTools()
        {
            List<Entry> list = new List<Entry>();
            foreach (Entry entry in entries)
            {
                if (entry.AppendedByEditor) continue;
                list.Add(entry);
            }
            return list;
        }

        /// <summary>The catalog entry for <paramref name="id"/>, or null.</summary>
        public static Entry? Find(string id)
        {
            foreach (Entry entry in entries)
            {
                if (entry.Id == id) return entry;
            }
            return null;
        }
    }
}
