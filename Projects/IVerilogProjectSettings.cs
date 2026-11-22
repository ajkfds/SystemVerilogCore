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
