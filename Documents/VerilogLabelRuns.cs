using System.Collections.Generic;
using System.Text;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Renders a <see cref="VerilogLabel"/> as a fenced markdown code block.
    ///
    /// <para>
    /// This is the only way a language-server host can show the parser's
    /// <em>coloured</em> label: VS Code renders a fenced block with the
    /// TextMate grammar of the given language id, while inline HTML styling is
    /// stripped by its markdown sanitizer (the allow list has no
    /// <c>style</c> / <c>class</c> attribute). Rendering the label's plain text
    /// into a fence therefore reproduces what the desktop editor paints with
    /// the same label runs, instead of a flat string with no colour.
    /// </para>
    ///
    /// <para>
    /// The run order, line breaks and trailing comment are kept exactly as the
    /// parser emitted them; only the palette indices are dropped (the grammar
    /// re-derives the colours). Runs that exist only as decoration (the net
    /// declaration icon) contribute no text, so nothing is lost by ignoring
    /// them.
    /// </para>
    /// </summary>
    public static class VerilogLabelRuns
    {
        /// <summary>
        /// Builds a fenced code block around the label's text.
        /// </summary>
        /// <param name="label">The label to render; null yields null.</param>
        /// <param name="language">
        /// TextMate language id used for the fence. The host must contribute a
        /// grammar for it, otherwise the block is not coloured.
        /// </param>
        /// <returns>
        /// The fenced block, or null when the label carries no text (so callers
        /// can fall back to a plain description).
        /// </returns>
        public static string? ToFencedCodeBlock(VerilogLabel? label, string language) =>
            label == null ? null : ToFencedCodeBlock(label.CreateString(), language);

        /// <summary>
        /// Builds a fenced code block around already-rendered text (used when a
        /// caller has a plain signature rather than a label).
        /// </summary>
        /// <returns>The fenced block, or null when the text is empty.</returns>
        public static string? ToFencedCodeBlock(string? text, string language)
        {
            if (string.IsNullOrEmpty(text)) return null;

            string body = NormalizeLineEndings(text).TrimEnd('\n', '\r');
            if (body.Length == 0) return null;

            StringBuilder sb = new StringBuilder();
            sb.Append("```");
            sb.Append(language);
            sb.Append('\n');
            sb.Append(body);
            sb.Append("\n```");
            return sb.ToString();
        }

        /// <summary>
        /// Collapses CR / CRLF to LF so the fence renders as one block per line
        /// (the parser emits CRLF in some labels).
        /// </summary>
        private static string NormalizeLineEndings(string text)
        {
            if (text.IndexOf('\r') < 0) return text;

            StringBuilder sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\r')
                {
                    sb.Append('\n');
                    if (i + 1 < text.Length && text[i + 1] == '\n') i++;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
