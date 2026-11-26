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
    }
}
