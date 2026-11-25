namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// The extra capability the parser needs from a source file, beyond what the
    /// editor's <c>ITextFile</c> already provides.
    ///
    /// <para>
    /// Parse sites used to reach this through the editor's concrete
    /// <c>Data.VerilogFile</c>:
    /// </para>
    ///
    /// <code>
    /// Data.IVerilogRelatedFile? file = projectProperty.DefinitionNameSpace.GetFile(name);
    /// if (file is not Data.VerilogFile) return null;
    /// Data.VerilogFile source = (Data.VerilogFile)file;
    /// ParsedDocument? parsed = source.GetInstancedVerilogParsedDocument(key);
    /// </code>
    ///
    /// <para>
    /// That cast is what kept the parser bound to the editor assembly. The only
    /// thing it actually bought was the instance-keyed parse lookup, so that is
    /// all this contract carries.
    /// </para>
    ///
    /// <para>
    /// <b>Why it is this small.</b> The editor's <c>Data.IVerilogRelatedFile</c>
    /// derives from <c>ITextFile</c>, which already declares <c>ID</c>,
    /// <c>RelativePath</c>, <c>Name</c>, <c>Project</c> and <c>ParsedDocument</c>.
    /// Re-declaring any of those on a second base interface makes every call
    /// site through the editor's file type fail with CS0229 (ambiguous member),
    /// and an interface cannot resolve that by re-declaring them. The same trap
    /// was hit once before, when <c>IVerilogRelatedFile</c> was pointed at
    /// <c>IVerilogDocumentFile</c>; the fix then was to keep the two hierarchies
    /// apart. This contract is therefore <em>additive only</em>: it declares
    /// just what <c>ITextFile</c> lacks, so a file type can implement it
    /// alongside <c>ITextFile</c> without any ambiguity.
    /// </para>
    /// </summary>
    public interface IVerilogSourceFile
    {
        /// <summary>
        /// The parse result produced for one parameter-overridden instance of a
        /// module in this file, or null when no such instance has been parsed.
        ///
        /// <para>
        /// A module instantiated as <c>child #(.WIDTH(4)) u(...)</c> is parsed
        /// again with those overrides applied, under a key derived from the
        /// module name and the overrides. Resolving that instance has to find
        /// <em>that</em> result, not the file's default-parameter parse, or the
        /// port widths and <c>generate</c> branches would be wrong. The key is
        /// built by the parser (<c>ParsedDocument.KeyGenerator</c>), so this
        /// lookup is the only thing the parser needs from the host here.
        /// </para>
        /// </summary>
        Parsing.IParsedDocument? GetInstancedParsedDocument(string instanceKey);
    }
}
