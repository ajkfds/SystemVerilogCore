namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Creates the include instances the parser records when it dives into an
    /// <c>include</c>.
    ///
    /// <para>
    /// An <c>include</c> produces a wrapper around the included file's buffer so
    /// the parent can colour it and re-parse it later. That wrapper is a host
    /// concept: the editor's <c>VerilogHeaderInstance</c> is an item-tree node
    /// with a project and a parent file, none of which the parser knows about.
    /// The parser therefore asks the host to make one instead of calling the
    /// editor's factory directly.
    /// </para>
    ///
    /// <para>
    /// The host supplies an implementation through
    /// <see cref="IVerilogProject.IncludeFileFactory"/>; a host that cannot
    /// create one (or has no project) leaves it null, in which case the parser
    /// reports the include as unresolved rather than crashing.
    /// </para>
    /// </summary>
    public interface IVerilogIncludeFileFactory
    {
        /// <summary>
        /// Creates an include instance for <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="relativePath">
        /// The include path as written, resolved against the project.
        /// </param>
        /// <param name="name">
        /// The include's display name (file name plus call index), used as the
        /// key in the parent's include table.
        /// </param>
        /// <param name="parentFile">
        /// The file that contains the <c>include</c>. The instance records it so
        /// the host can find the including file again.
        /// </param>
        /// <param name="instancedReference">
        /// Where the <c>include</c> directive appeared, as an opaque token the
        /// parser already had. The host stores it on the instance so the editor
        /// can match a re-parsed include back to the one it is showing. The
        /// parser owns the token; this contract only passes it through.
        /// </param>
        /// <param name="id">Id to assign to the instance.</param>
        /// <returns>
        /// The instance, or null when the included file cannot be resolved. Null
        /// is the normal "file not found" answer: the parser reports it as an
        /// error on the include directive.
        /// </returns>
        IVerilogIncludeFile? Create(
            string relativePath,
            string name,
            IVerilogSourceFile parentFile,
            object? instancedReference,
            string id);
    }
}
