using System.Collections.Generic;

namespace SystemVerilogCore.Parsing
{
    /// <summary>
    /// Lets a parse result expose its diagnostics without the host needing the
    /// parser's concrete message type.
    ///
    /// <para>
    /// The editor's Messages pane renders a row per message, and the language
    /// server maps them to LSP diagnostics. Both used to read a
    /// <c>ParsedDocument.Messages</c> list whose element type lived in the
    /// editor assembly. The messages travel as <see cref="object"/> on purpose:
    /// each plugin owns its message type, and the host only forwards it to a
    /// row builder it was given (the editor's <c>MessageNodeFactory</c>).
    /// </para>
    ///
    /// <para>
    /// Implemented by the parser's parse result, so the host can ask any parse
    /// result for its messages without referencing the parser.
    /// </para>
    /// </summary>
    public interface IParsedDocumentMessagesProvider
    {
        /// <summary>
        /// Diagnostics produced by the last parse, in the order they were
        /// reported.
        /// </summary>
        IReadOnlyList<object> Messages { get; }
    }
}
