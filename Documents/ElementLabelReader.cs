using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Exposes the parser's element labels to a host that can only render
    /// markdown (the language server), plus the palette those labels refer to.
    ///
    /// <para>
    /// The desktop editor paints a mouse-over hint by turning the label's runs
    /// into a <c>ColorLabel</c>. A markdown host has no such primitive: VS
    /// Code's hover sanitizer strips <c>style</c> / <c>class</c>, so the colours
    /// can only come from a fenced code block that a TextMate grammar tokenises.
    /// This supplies both halves - the label as text (through
    /// <see cref="VerilogLabelRuns"/>) and the palette the host needs to declare
    /// a matching grammar.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.ElementLabelAdapter</c>) and read the
    /// palette off the Avalonia-based <c>CodeDrawStyle</c>, so the language
    /// server had to reference the editor stack to render a hover. The parser
    /// already builds the labels, and the palette exists here as plain
    /// <c>#RRGGBB</c> strings, so nothing needs a host type.
    /// </para>
    /// </summary>
    public static class ElementLabelReader
    {
        /// <summary>
        /// The TextMate language id the labels are rendered with.
        ///
        /// <para>
        /// The extension contributes this id with a generated grammar (same
        /// colours as the parser palette). It is a dedicated id so the grammar
        /// only ever affects hover code blocks and never the editor's own
        /// highlighting.
        /// </para>
        /// </summary>
        public const string LabelLanguage = "systemverilog-hover";

        /// <summary>
        /// Builds the coloured-markdown rendering of an element's declaration
        /// label (the same label the editor's hint shows).
        /// </summary>
        /// <returns>
        /// A fenced code block, or null when the element has no label (the
        /// caller then falls back to a plain description).
        /// </returns>
        public static string? BuildLabelMarkdown(Verilog.INamedElement? element)
        {
            VerilogLabel? label = BuildLabel(element);
            if (label == null) return null;
            return VerilogLabelRuns.ToFencedCodeBlock(label, LabelLanguage);
        }

        /// <summary>
        /// The plain signature the editor's hint would show, for elements that
        /// have no label of their own (building blocks). Rendered in the same
        /// fence as the labels so every hover is coloured the same way.
        /// </summary>
        /// <returns>The fenced block, or null when the element has no name.</returns>
        public static string? BuildSignatureMarkdown(ISystemVerilogNamedElement? element)
        {
            if (element == null) return null;
            if (string.IsNullOrEmpty(element.Name)) return null;

            return VerilogLabelRuns.ToFencedCodeBlock(
                HoverContent.BuildSignature(element), LabelLanguage);
        }

        /// <summary>
        /// The label the editor's mouse-over hint renders for this element, or
        /// null when the element type has no label.
        /// </summary>
        public static VerilogLabel? BuildLabel(Verilog.INamedElement? element)
        {
            if (element == null) return null;

            // Mirrors ParsedDocument.GetHintContent: only these element kinds
            // carry a hint label in the editor.
            try
            {
                VerilogLabel label = new();
                switch (element)
                {
                    case Verilog.DataObjects.DataObject dataObject:
                        dataObject.AppendLabel(label);
                        return label;
                    case Verilog.Function function:
                        function.AppendLabel(label);
                        return label;
                    case Verilog.Task_ task:
                        task.AppendLabel(label);
                        return label;
                    case Verilog.DataObjects.Typedef typedef:
                        typedef.AppendLabel(label);
                        return label;
                    default:
                        return null;
                }
            }
            catch
            {
                // A hint must never fail the hover request: an unresolved
                // cross-project element can throw while describing itself.
                return null;
            }
        }

        /// <summary>
        /// The palette slots the labels use, keyed by the slot's name, as
        /// <c>#RRGGBB</c>. A host declares a TextMate grammar with these colours
        /// so the fenced label matches the desktop editor.
        /// </summary>
        public static Dictionary<string, string> GetPalette()
        {
            Dictionary<string, string> palette = new();
            foreach (VerilogColorType colorType in VerilogColorPalette.All)
            {
                palette[colorType.ToString()] = VerilogColorPalette.ToHex(colorType);
            }
            return palette;
        }
    }
}
