using System.Collections.Generic;
using SystemVerilogCore.Documents;

namespace SystemVerilogCore.Diagnostics
{
    /// <summary>
    /// Converts the parser's colour spans into a flat list of
    /// (index, length, palette) segments that UI-agnostic hosts (LSP semantic
    /// tokens) can consume.
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.ColorSegmentAdapter</c>) and took the
    /// plugin's own <c>CodeDrawStyle.ColorType</c>, so the language server had
    /// to reference the editor stack to read colours. The spans come straight
    /// off the document contract now
    /// (<see cref="IVerilogTextDocument.GetColorSpans"/>), and the palette slot
    /// is the contract enum <see cref="VerilogColorType"/>, so nothing here
    /// needs a host type.
    /// </para>
    /// </summary>
    public static class ColorSegmentReader
    {
        /// <summary>A flat colour segment (document-absolute index range).</summary>
        public readonly struct Segment
        {
            public Segment(int index, int length, VerilogColorType type)
            {
                Index = index;
                Length = length;
                Type = type;
            }

            /// <summary>Document-absolute start offset.</summary>
            public int Index { get; }

            /// <summary>Span length in characters.</summary>
            public int Length { get; }

            /// <summary>Palette slot (see <see cref="VerilogColorType"/>).</summary>
            public VerilogColorType Type { get; }
        }

        /// <summary>
        /// Returns the parser's colour spans as flat segments. The default
        /// colour (Normal) is skipped so hosts can layer their own.
        /// </summary>
        public static List<Segment> GetSegments(IVerilogTextDocument? document)
        {
            List<Segment> segments = new List<Segment>();
            if (document == null) return segments;

            foreach (VerilogColorSpan span in document.GetColorSpans())
            {
                VerilogColorType type = (VerilogColorType)span.ColorIndex;
                if (type == VerilogColorType.Normal) continue; // default colour, host decides
                segments.Add(new Segment(span.Index, span.Length, type));
            }
            return segments;
        }
    }
}
