using System.Collections.Generic;
using System.Text;

namespace SystemVerilogCore.Diagnostics
{
    /// <summary>
    /// Maps parser-emitted message text to the LSP-friendly diagnostic
    /// <c>code</c> field.
    ///
    /// <para>
    /// The parser does not store a code on each message, so one is inferred
    /// from a small list of well-known substrings (<c>"undriven"</c>,
    /// <c>"unused"</c>, ...) with a normalised slug of the message text as the
    /// fallback. The slug uses a stable prefix so client-side rule suppressions
    /// can be written against it.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.DiagnosticCodeMap</c>), which meant the
    /// language server had to reference the editor stack to give a diagnostic a
    /// code. Nothing here needs the UI: the input is a string.
    /// </para>
    /// </summary>
    public static class DiagnosticCodeMap
    {
        private const string Prefix = "verilog";

        // Substring → code. Matched case-insensitively against the message
        // text. Keys are ordered most-specific-first; the first match wins.
        private static readonly (string Needle, string Code)[] KnownSubstrings =
        {
            ("undriven", "verilog/undriven"),
            ("unused", "verilog/unused"),
            ("not defined here", "verilog/undefined"),
            ("undefined", "verilog/undefined"),
            ("not found", "verilog/undefined"),
            ("duplicate", "verilog/duplicate"),
            ("duplicated", "verilog/duplicate"),
            ("already declared", "verilog/duplicate"),
            ("implicit net", "verilog/implicit-net-declaration"),
            ("implicit modport", "verilog/implicit-modport"),
            ("bitwidth", "verilog/assignment-bitwidth-mismatch"),
            ("not all ports connected", "verilog/not-all-ports-connected"),
            ("syntax", "verilog/syntax"),
        };

        /// <summary>The code for <paramref name="text"/>.</summary>
        public static string FromMessage(string? text)
        {
            if (string.IsNullOrEmpty(text)) return Prefix + "/unknown";

            foreach ((string needle, string code) in KnownSubstrings)
            {
                if (text.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return code;
                }
            }

            return Prefix + "/" + Slug(text);
        }

        private static string Slug(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            bool lastWasDash = false;
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                    lastWasDash = false;
                }
                else if (c == ' ' || c == '_' || c == '-')
                {
                    if (!lastWasDash && sb.Length > 0)
                    {
                        sb.Append('-');
                        lastWasDash = true;
                    }
                }
                // Skip other characters; they are not part of the slug.

                if (sb.Length >= 32) break;
            }

            // Trim trailing dash.
            if (sb.Length > 0 && sb[sb.Length - 1] == '-') sb.Length--;
            if (sb.Length == 0) return "info";
            return sb.ToString();
        }
    }
}
