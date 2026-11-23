using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// One diagnostic line to show inside a mouse-over hint.
    ///
    /// <para>
    /// The hint is "the messages at this position, then the element's
    /// declaration label". The message lines are decorated by the host (the
    /// editor draws a coloured triangle icon and uses its own colours, which are
    /// not palette entries), so only the plain data travels here: the severity
    /// and the text.
    /// </para>
    /// </summary>
    public sealed class VerilogHintMessage
    {
        public VerilogHintMessage(VerilogHintSeverity severity, string text)
        {
            Severity = severity;
            Text = text;
        }

        /// <summary>Severity, which decides the host's colour / icon.</summary>
        public VerilogHintSeverity Severity { get; }

        /// <summary>The message text (no trailing newline).</summary>
        public string Text { get; }
    }

    /// <summary>
    /// Severity of a <see cref="VerilogHintMessage"/>.
    ///
    /// <para>
    /// Mirrors the parser's message types so a host can pick its own colours
    /// without referencing the parser's <c>Message</c> class.
    /// </para>
    /// </summary>
    public enum VerilogHintSeverity
    {
        Error,
        Warning,
        Notice,
        Hint,
    }

    /// <summary>
    /// The data a mouse-over hint is built from: the messages at the position
    /// and the element's declaration label.
    ///
    /// <para>
    /// This is what the parser can produce on its own. Turning it into a hint
    /// popup (colours, icons, text spans) is the host's job - the editor draws
    /// it, a language server renders the same data as markdown. Keeping the two
    /// apart is what lets the parser drop its last reference to the editor's
    /// <c>PopupItem</c>.
    /// </para>
    /// </summary>
    public sealed class VerilogHintContent
    {
        /// <summary>Diagnostic lines to show first, in declaration order.</summary>
        public List<VerilogHintMessage> Messages { get; } = new();

        /// <summary>
        /// The element's declaration label (palette indices, no colours), or
        /// null when the position has no labelled element.
        /// </summary>
        public VerilogLabel? Label { get; set; }
    }
}
