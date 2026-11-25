namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A file pulled in by an <c>include</c> directive.
    ///
    /// <para>
    /// The parser records one of these per <c>include</c> so a later pass can
    /// re-parse the included file together with its parent, and so the parent's
    /// colour table can be refreshed from it. The parser only ever needs the
    /// identity, the include's parse result and a way to release it, so that is
    /// all this contract carries.
    /// </para>
    ///
    /// <para>
    /// <b>Why so little.</b> The editor's <c>Data.VerilogHeaderInstance</c> is an
    /// <c>InstanceTextFile</c> with an item-tree node, a project and a parent
    /// file; none of that is used while parsing. Keeping the contract narrow
    /// means the include table can live in the UI-free assembly while the editor
    /// keeps its richer instance type (it implements this alongside its own
    /// base). Editor-only work (refreshing the NavigatePanel item tree, copying
    /// colour marks into the displayed document) narrows back to that type in the
    /// data layer.
    /// </para>
    ///
    /// <para>
    /// The member names are distinct from the editor's on purpose: an interface
    /// that re-declared <c>ID</c> or <c>ParsedDocument</c> would make every call
    /// site through the editor's file types ambiguous (CS0229).
    /// </para>
    /// </summary>
    public interface IVerilogIncludeFile
    {
        /// <summary>Unique id of the include instance (host-defined).</summary>
        string IncludeId { get; }

        /// <summary>
        /// The include's parse result, or null when it has not been parsed yet.
        /// </summary>
        Parsing.IParsedDocument? IncludeParsedDocument { get; }

        /// <summary>
        /// The included file, viewed as a source file.
        ///
        /// <para>
        /// The parser builds the include's own parse result from this: it needs
        /// the buffer to snapshot and the identity for the cache key. An include
        /// wrapper is not itself a source file on every host (the language
        /// server's wrapper is not), so it hands one out rather than deriving
        /// from the contract.
        /// </para>
        /// </summary>
        IVerilogSourceFile? SourceFile { get; }

        /// <summary>
        /// Where the <c>include</c> directive appeared, as the opaque token the
        /// parser handed to the factory.
        ///
        /// <para>
        /// The prototype pass records the include, and the real pass has to find
        /// that same entry again by position (an include can appear twice in one
        /// file, so the path alone is not unique). The parser compares these
        /// tokens with <c>IndexReference.IsSameAs</c>; the contract only carries
        /// them, it does not interpret them.
        /// </para>
        /// </summary>
        object? InstancedReference { get; }

        /// <summary>
        /// Releases the include's buffer and parse result.
        ///
        /// <para>
        /// Called when the parent is re-parsed: the includes are re-created from
        /// scratch, so the old ones must not keep their buffers alive.
        /// </para>
        /// </summary>
        void CloseInclude();
    }
}
