using System.Text.Json;

namespace SystemVerilogCore.Projects
{
    /// <summary>
    /// Per-project settings that the parser reads while parsing.
    ///
    /// <para>
    /// The editor calls this a <c>ProjectProperty</c> and the language server
    /// maps it to "settings for this workspace folder", but both need the same
    /// thing from the parser's point of view: a bag of settings that belongs to
    /// one project and outlives the parse.
    /// </para>
    ///
    /// <para>
    /// This interface deliberately contains only what the parser consumes. The
    /// editor's property-edit UI (the tab that renders these settings) is a UI
    /// concern and stays in the editor.
    /// </para>
    /// </summary>
    public interface IVerilogProjectSettings
    {
        /// <summary>
        /// Identifier used when the settings are serialised into the project
        /// file. Plugins use it to find their own section.
        /// </summary>
        string ID { get; }

        // ---- what the parser reads while parsing -------------------------
        //
        // These were previously reached by naming the editor's
        // `pluginVerilog.ProjectProperty` at every parse site, which is what kept
        // the parser bound to the plugin. The values are the same; only the
        // static type changed.

        /// <summary>
        /// Top-level module / interface / program / primitive declarations.
        /// </summary>
        Verilog.IVerilogNameSpace DefinitionNameSpace { get; }

        /// <summary>Package declarations.</summary>
        Verilog.IVerilogNameSpace PackageNameSpace { get; }

        /// <summary>
        /// Compilation-unit-scope declarations (root-level classes, functions,
        /// tasks, typedefs).
        /// </summary>
        Verilog.IVerilogNameSpace UnitNameSpace { get; }

        /// <summary>
        /// The `` `define `` macros of this project, by name.
        /// </summary>
        IReadOnlyDictionary<string, Verilog.Macro> Macros { get; }

        /// <summary>
        /// The comment annotation command words (<c>@sync</c>, <c>@clock</c>, ...).
        /// </summary>
        Verilog.AnnotationCommands AnnotationCommands { get; }

        /// <summary>
        /// The default rule set (severity settings) of this project.
        /// </summary>
        Verilog.RuleSet RuleSet { get; }

        /// <summary>
        /// Modules provided by an external library, mapped to their library path.
        /// Such a module is not an error when it is not found in the project.
        /// </summary>
        IReadOnlyDictionary<string, string> ExtenralModuleLibraryPath { get; }

        /// <summary>
        /// Primitives (UDPs) provided by an external library, mapped to their
        /// library path.
        /// </summary>
        IReadOnlyDictionary<string, string> ExtenralPrimitiveLibraryPath { get; }
    }

    /// <summary>
    /// A project property object that can be attached to a project and
    /// serialised with it.
    ///
    /// <para>
    /// Kept separate from <see cref="IVerilogProjectSettings"/> so that a host
    /// which has no project file (the language server) can implement only the
    /// settings surface, while the editor implements both.
    /// </para>
    /// </summary>
    public interface IVerilogSerializableProjectSettings : IVerilogProjectSettings
    {
        /// <summary>
        /// Writes this settings object as JSON. Called when the project file is
        /// saved.
        /// </summary>
        void Write(Utf8JsonWriter writer, JsonSerializerOptions options);
    }
}
