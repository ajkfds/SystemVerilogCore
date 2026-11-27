namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A source file that can hold parse results for individual
    /// parameter-overridden instances of its modules.
    ///
    /// <para>
    /// A module instantiated as <c>child #(.WIDTH(4)) u(...)</c> is parsed again
    /// with those overrides applied, under a key derived from the module name and
    /// the overrides (<see cref="Parsing.VerilogParseMode"/> aside, the key comes
    /// from <c>ParsedDocument.KeyGenerator</c>). Reading that instance back has to
    /// find <em>that</em> result, not the file's default-parameter parse.
    /// </para>
    ///
    /// <para>
    /// <see cref="IVerilogSourceFile.GetInstancedParsedDocument"/> is the read
    /// side. This is the write side, which only a host that drives a parse itself
    /// needs - the editor fills the same table while accepting a parse result. A
    /// host with no such table simply does not implement this and the write is
    /// skipped.
    /// </para>
    /// </summary>
    public interface IVerilogSourceFileInstanceCache
    {
        /// <summary>
        /// Records <paramref name="parsedDocument"/> as the parse result of the
        /// instance identified by <paramref name="instanceKey"/>.
        /// </summary>
        void RegisterInstanceParsedDocument(string instanceKey, Parsing.IParsedDocument parsedDocument);

        /// <summary>
        /// Drops every recorded instance result.
        ///
        /// <para>
        /// A file's text changed, so every parameter-overridden parse it produced
        /// for its instances describes the old text. A host that refreshes a
        /// buffer (the language server on <c>didChange</c>) has to call this, or
        /// an expand under a parent keeps showing the previous instantiations.
        /// The editor drops them while accepting the new parse result; a host
        /// that drives its own buffer has no such hook.
        /// </para>
        /// </summary>
        void ClearInstanceParsedDocuments();
    }

    /// <summary>
    /// A source file that can hold its own parse result.
    ///
    /// <para>
    /// The editor stores a parse result on the file while accepting it
    /// (<c>TextFile.AcceptParsedDocumentAsync</c>), and everything downstream
    /// reads it back through <see cref="IVerilogSourceFile.VerilogParsedResult"/>:
    /// the hierarchy update, the completion / snippet adapters, the token
    /// adapter. A host that drives a parse itself has to fill the same slot, or
    /// those readers see no parse result at all.
    /// </para>
    ///
    /// <para>
    /// <see cref="IVerilogSourceFile.VerilogParsedResult"/> is the read side; this
    /// is the write side. A host with no such slot simply does not implement it
    /// and the write is skipped.
    /// </para>
    /// </summary>
    public interface IVerilogSourceFileParseCache
    {
        /// <summary>
        /// Stores <paramref name="parsedDocument"/> as this file's parse result.
        /// </summary>
        void SetVerilogParsedResult(Parsing.IParsedDocument parsedDocument);
    }
}
