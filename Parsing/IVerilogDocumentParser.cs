using System.Threading;
using System.Threading.Tasks;

namespace SystemVerilogCore.Parsing
{
    /// <summary>
    /// The minimum a parser must expose so a host can drive it without knowing
    /// the host's own parser base class.
    ///
    /// <para>
    /// The SystemVerilog parser used to call the editor's
    /// <c>DocumentParser</c> directly when it had to parse a file on demand
    /// (the parameter-overridden instance in <c>ModuleInstantiation</c> and the
    /// <c>@scope</c> target file in <c>CommentAnnotationItem</c>). That single
    /// type reference was the last thing tying the parser to the editor
    /// assembly for parse-mode purposes: the parser only ever needed to create
    /// a parser, run it and read its result.
    /// </para>
    ///
    /// <para>
    /// This contract carries exactly that. The editor's <c>DocumentParser</c>
    /// implements it, and each host exposes a factory that returns it (see
    /// <c>pluginVerilog.Data.IVerilogRelatedFile.CreateVerilogDocumentParser</c>),
    /// so the parser no longer names the editor type. The parse mode travels as
    /// <see cref="VerilogParseMode"/> - the shared enum - rather than the
    /// editor's nested alias of it.
    /// </para>
    /// </summary>
    public interface IVerilogDocumentParser
    {
        /// <summary>Why this document is being parsed.</summary>
        VerilogParseMode ParseMode { get; }

        /// <summary>
        /// The parse result, or null until <see cref="ParseAsync()"/> finished.
        /// </summary>
        IParsedDocument? ParsedDocument { get; }

        /// <summary>Runs the parse.</summary>
        Task ParseAsync();

        /// <summary>Runs the parse, observing the given cancellation token.</summary>
        Task ParseAsync(CancellationToken? cancellationToken);
    }
}
