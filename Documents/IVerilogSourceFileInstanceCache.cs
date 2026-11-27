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
    /// <summary>
    /// A file that can hand out the instance items it registered.
    ///
    /// <para>
    /// <c>SimulationSetup</c> walks the instance tree to collect the files a
    /// simulator would compile, and it looks an instance up by name because the
    /// INSTANCE - not the definition file - carries the parameter-overridden
    /// parse. The editor's items live in <c>Data.Item.Items</c>; a host without
    /// that tree answers from whatever it has (the language server derives the
    /// nodes on demand).
    /// </para>
    ///
    /// <para>
    /// Returning null means "this host has no instance items", in which case the
    /// caller falls back to the definition file's own parse.
    /// </para>
    /// </summary>
    public interface IVerilogInstanceItemSource
    {
        /// <summary>
        /// The instance item registered under <paramref name="name"/>, or null.
        /// </summary>
        /// <remarks>
        /// The name is the instantiation's name (or "name[i]" for an array
        /// element). The returned object is host-defined: the caller narrows it
        /// to the source-file contract to read its parse result.
        /// </remarks>
        object? FindInstanceItem(string name);
    }

    /// <summary>
    /// A host that records the instances it parsed, so a later walk can find
    /// them by name.
    ///
    /// <para>
    /// The editor records an instance as an item-tree node
    /// (<c>VerilogModuleInstance</c>) and the tree answers lookups by name. A
    /// host without a tree still has to answer them: the simulation setup walks
    /// instances to collect the files a simulator would compile, and an include
    /// inside an instance body resolves through the instance's own parse. This
    /// contract is that recording step, without the item tree.
    /// </para>
    ///
    /// <para>
    /// Implementations keep the parse result keyed by the instance name (or
    /// "name[i]" for an array element), which is the name the callers use.
    /// </para>
    /// </summary>
    public interface IVerilogInstanceItemSourceProvider
    {
        /// <summary>
        /// Records that <paramref name="instanceName"/> was parsed as
        /// <paramref name="parsedDocument"/>.
        /// </summary>
        void RegisterInstanceItem(string instanceName, Parsing.IParsedDocument parsedDocument);
    }

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
