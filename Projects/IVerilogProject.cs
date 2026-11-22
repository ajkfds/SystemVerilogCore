namespace SystemVerilogCore.Projects
{
    /// <summary>
    /// A project (workspace) as the parser sees it: the root of an item tree
    /// plus the per-project settings.
    ///
    /// <para>
    /// The editor calls this a <c>Project</c>; the language server maps it to a
    /// workspace folder. Both need the same three things while parsing:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>a name, used by <c>// @project &lt;name&gt;</c> to reference a
    /// different project's modules,</item>
    /// <item>the project's settings (<c>ProjectProperty</c>: macros, system
    /// functions, name spaces, rule set, ...),</item>
    /// <item>the root of the item tree, so <c>`include</c> can be resolved.</item>
    /// </list>
    ///
    /// <para>
    /// This assembly stays UI-free: no Avalonia, no editor types.
    /// </para>
    /// </summary>
    public interface IVerilogProject
    {
        /// <summary>
        /// Project name. Used as the key of <see cref="IVerilogProjectResolver"/>
        /// and as the value written in a <c>// @project &lt;name&gt;</c>
        /// annotation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The root of this project's item tree.
        /// </summary>
        IVerilogItemTree Items { get; }

        /// <summary>
        /// This project's parser settings, or null when none are configured.
        ///
        /// <para>
        /// The editor's <c>Project.ProjectProperties["Verilog"]</c> and the
        /// language server's per-workspace settings both surface here, so a
        /// parse site can reach the settings without knowing which host it runs
        /// in.
        /// </para>
        /// </summary>
        IVerilogProjectSettings? Settings { get; }

        /// <summary>
        /// Resolves another loaded project by name, for
        /// <c>// @project &lt;name&gt;</c> references.
        ///
        /// <para>
        /// Returns null when no project with that name is loaded, in which case
        /// the caller keeps using the local project. Hosts implement this over
        /// their own project registry (the editor's <c>Global.Projects</c>, the
        /// language server's per-workspace-folder projects).
        /// </para>
        /// </summary>
        IVerilogProject? ResolveProject(string name);
    }

    /// <summary>
    /// Resolves a project by name, i.e. the replacement for the editor's
    /// process-wide <c>Global.Projects</c> dictionary.
    ///
    /// <para>
    /// <c>// @project &lt;name&gt;</c> lets one project reference a module that
    /// lives in another. The parser therefore needs to look a project up by
    /// name. The editor implements this over <c>Global.Projects</c>; the
    /// language server implements it over its per-workspace-folder projects.
    /// </para>
    /// </summary>
    public interface IVerilogProjectResolver
    {
        /// <summary>
        /// Returns the project with the given name, or null when it is not
        /// loaded. A null result means the cross-project reference cannot be
        /// resolved (the caller falls back to the local project).
        /// </summary>
        IVerilogProject? FindProject(string name);
    }

    /// <summary>
    /// The item tree of a project: a root item that can be searched.
    ///
    /// <para>
    /// The parser only ever asks two questions of it, both from
    /// <c>`include</c> resolution:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>find the file at a project-relative path (same-folder
    /// include),</item>
    /// <item>find a file by name anywhere in the project (fallback when the
    /// include path has a directory prefix).</item>
    /// </list>
    /// </summary>
    public interface IVerilogItemTree
    {
        /// <summary>
        /// Returns the item at the given project-relative path, or null.
        /// The path is '/'-separated.
        /// </summary>
        IVerilogItem? FindByRelativePath(string relativePath);

        /// <summary>
        /// Returns a file item whose name matches <paramref name="fileName"/>,
        /// or null. Used as the include fallback.
        /// </summary>
        IVerilogItem? FindByName(string fileName);
    }

    /// <summary>
    /// An item in a project's tree: a folder or a file.
    /// </summary>
    public interface IVerilogItem
    {
        /// <summary>Display name (usually the file name).</summary>
        string Name { get; }

        /// <summary>Project-relative path, '/'-separated.</summary>
        string RelativePath { get; }
    }
}
