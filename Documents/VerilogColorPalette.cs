using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// The semantic colour palette, as plain <c>#RRGGBB</c> strings.
    ///
    /// <para>
    /// <b>Why this lives here.</b> The editor's palette
    /// (<c>pluginVerilog.CodeDrawStyle</c>) is built on <c>Avalonia.Media.Color</c>,
    /// so a host that wants the editor's colours - the language server hands them
    /// to the client as theme rules - had to reference the whole Avalonia stack
    /// just to read a dozen RGB values. This is the same table without the UI
    /// type, so the colours have a single source that any host can read.
    /// </para>
    ///
    /// <para>
    /// The indices are the editor's palette slots and must not be renumbered:
    /// they are written into documents as raw bytes
    /// (<see cref="VerilogColorType"/>), so a change here would recolour every
    /// cached parse result.
    /// </para>
    /// </summary>
    public static class VerilogColorPalette
    {
        /// <summary>
        /// The colour of <paramref name="colorType"/> as <c>#RRGGBB</c>.
        /// </summary>
        public static string ToHex(VerilogColorType colorType) =>
            colorType switch
            {
                VerilogColorType.Normal => "#D4D4D4",
                VerilogColorType.Inactivated => "#969696",
                VerilogColorType.Register => "#FF3232",
                VerilogColorType.Keyword => "#569CD6",
                VerilogColorType.Comment => "#6A9955",
                VerilogColorType.Identifier => "#4EC9B0",
                VerilogColorType.Parameter => "#FF5EC2",
                VerilogColorType.Number => "#CE9178",
                VerilogColorType.Net => "#FFC8C8",
                VerilogColorType.HighLightedComment => "#C8FF64",
                VerilogColorType.Variable => "#FFB4B4",
                VerilogColorType.CommentAnnotation => "#C8FF64",
                _ => "#D4D4D4",
            };

        /// <summary>
        /// Every palette entry, in slot order.
        ///
        /// <para>
        /// Hosts that publish the palette (the language server sends these as
        /// semantic-token colour rules) iterate this rather than hard-coding the
        /// table a second time.
        /// </para>
        /// </summary>
        public static IReadOnlyList<VerilogColorType> All { get; } = new[]
        {
            VerilogColorType.Normal,
            VerilogColorType.Inactivated,
            VerilogColorType.Register,
            VerilogColorType.Keyword,
            VerilogColorType.Comment,
            VerilogColorType.Identifier,
            VerilogColorType.Parameter,
            VerilogColorType.Number,
            VerilogColorType.Net,
            VerilogColorType.HighLightedComment,
            VerilogColorType.Variable,
            VerilogColorType.CommentAnnotation,
        };
    }
}
