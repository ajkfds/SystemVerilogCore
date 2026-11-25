namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// The extra capabilities the parser needs from a source file, beyond what
    /// the editor's <c>ITextFile</c> already provides.
    ///
    /// <para>
    /// Parse sites used to reach these through the editor's concrete
    /// <c>Data.VerilogFile</c>:
    /// </para>
    ///
    /// <code>
    /// Data.IVerilogRelatedFile? file = ...;
    /// if (file is not Data.VerilogFile) return null;
    /// Data.VerilogFile source = (Data.VerilogFile)file;
    /// ParsedDocument? parsed = source.GetInstancedVerilogParsedDocument(key);
    /// </code>
    ///
    /// <para>
    /// That cast is what kept the parser bound to the editor assembly.
    /// </para>
    ///
    /// <para>
    /// <b>Why the member names differ from <c>ITextFile</c>.</b> The editor's
    /// <c>Data.IVerilogRelatedFile</c> derives from <c>ITextFile</c>, which
    /// already declares <c>ID</c>, <c>RelativePath</c>, <c>Name</c>,
    /// <c>Project</c>, <c>ParsedDocument</c> and <c>CodeDocument</c>. Declaring
    /// any of those again on a second base interface makes every call site
    /// through the editor's file type fail with CS0229 (ambiguous member), and
    /// an interface cannot resolve that by re-declaring them. The same trap was
    /// hit twice while moving the parser (see the git history), so this contract
    /// uses distinct names for the same values: <see cref="VerilogTextDocument"/>
    /// rather than <c>CodeDocument</c>, <see cref="ProjectSettings"/> rather than
    /// <c>ProjectProperty</c>, and so on. Implementations forward to their
    /// existing members, so the host keeps its <c>ITextFile</c> base and no call
    /// site changes meaning.
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

        /// <summary>
        /// This file's own parse result, or null when it has not been parsed yet.
        ///
        /// <para>
        /// Used as the fallback when a parameter-overridden instance result is not
        /// available: the base parse is what the instance would otherwise
        /// resolve against. Same value as the editor's
        /// <c>IVerilogRelatedFile.VerilogParsedDocument</c>, under a name that does
        /// not collide with <c>ITextFile.ParsedDocument</c>.
        /// </para>
        /// </summary>
        Parsing.IParsedDocument? VerilogParsedResult { get; }

        /// <summary>
        /// The document the parser reads and colours, or null when the file has
        /// no buffer yet.
        ///
        /// <para>
        /// The parser tokenises this document and writes colour spans into it.
        /// Same value as the editor's <c>ITextFile.CodeDocument</c>, under a name
        /// that does not collide with it.
        /// </para>
        /// </summary>
        IVerilogTextDocument? VerilogTextDocument { get; }

        /// <summary>
        /// Version of the buffer <see cref="VerilogTextDocument"/> returned when
        /// it was handed out.
        ///
        /// <para>
        /// A parse result records this so a host can tell whether the document
        /// has been edited since (a stale result is re-parsed). Kept as a plain
        /// number so the contract does not depend on the editor's document type.
        /// </para>
        /// </summary>
        ulong VerilogTextDocumentVersion { get; }

        /// <summary>
        /// The parser settings of this file's project, or null when the file is
        /// not attached to a project yet.
        ///
        /// <para>
        /// Same value as the editor's <c>ProjectProperty</c>, under a name that
        /// does not collide with it. Carries the name spaces, macros, system task
        /// tables and rule set.
        /// </para>
        /// </summary>
        Projects.IVerilogProjectSettings? ProjectSettings { get; }

        /// <summary>
        /// The project this file belongs to, or null.
        ///
        /// <para>
        /// Needed for <c>// @project &lt;name&gt;</c> resolution and for building
        /// parse-cache keys (the key starts with the project name).
        /// </para>
        /// </summary>
        Projects.IVerilogProject? VerilogProject { get; }

        /// <summary>
        /// Unique id of the file (host-defined).
        ///
        /// <para>
        /// Same value as the editor's <c>ITextFile.ID</c>. Spelled with a lower
        /// case `d` so it does not collide with that member (a re-declaration
        /// would make every call site through the editor's file types ambiguous).
        /// </para>
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Project-relative path, '/'-separated.
        ///
        /// <para>
        /// Part of the parse-cache key. Same value as
        /// <c>ITextFile.RelativePath</c>, under a name that does not collide.
        /// </para>
        /// </summary>
        string FileRelativePath { get; }

        /// <summary>
        /// True when the file is SystemVerilog (as opposed to Verilog-2001).
        /// A few constructs are only legal in one of the two.
        /// </summary>
        bool IsSystemVerilog { get; }
    }
}
