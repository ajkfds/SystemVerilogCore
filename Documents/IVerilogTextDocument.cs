using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Abstraction over the text document the parser reads and colourises.
    ///
    /// <para>
    /// This is the seam that lets the SystemVerilog parser live without the
    /// editor's UI stack. The parser only needs four operations from a
    /// document - its length, the character at an offset, a substring, and a
    /// way to record a colour span - so the interface deliberately exposes
    /// exactly those and nothing else. Hosts supply an implementation:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>the desktop editor wraps its AvaloniaEdit-backed document,</item>
    /// <item>the language server / tests use a plain string-backed document.</item>
    /// </list>
    ///
    /// <para>
    /// <see cref="ISystemVerilogCodeDocument"/> (the read-only surface used by
    /// symbol queries) is a separate, narrower interface: consumers that only
    /// read text should depend on that one, while the parser needs this one
    /// because it also writes colour spans.
    /// </para>
    /// </summary>
    public interface IVerilogTextDocument
    {
        /// <summary>Total length of the document in characters.</summary>
        int Length { get; }

        /// <summary>
        /// Returns the character at <paramref name="index"/>, or <c>'\0'</c>
        /// when the index is outside the document. Returning a sentinel rather
        /// than throwing keeps the tokenizer's look-ahead simple: it probes one
        /// character past the end constantly.
        /// </summary>
        char GetCharAt(int index);

        /// <summary>
        /// Returns <paramref name="length"/> characters starting at
        /// <paramref name="startIndex"/>. Out-of-range requests are clamped.
        /// </summary>
        string CreateString(int startIndex, int length);

        /// <summary>
        /// Records that <paramref name="length"/> characters from
        /// <paramref name="index"/> are painted with the palette entry
        /// <paramref name="colorIndex"/> (an index into the host's colour
        /// palette, see <c>IVerilogColorPalette</c>).
        /// </summary>
        void SetColorAt(int index, byte colorIndex, int length);
    }

    /// <summary>
    /// The colour palette the parser writes into a document.
    ///
    /// <para>
    /// The parser never needs the actual colour values - it only needs the
    /// stable byte index of a semantic category (keyword, comment, net, ...)
    /// so the host can paint it. Keeping the index contract here means the
    /// parser does not have to know about Avalonia's <c>Color</c>.
    /// </para>
    ///
    /// <para>
    /// The concrete indices match the editor's palette layout
    /// (<c>pluginVerilog.CodeDrawStyle.ColorType</c>) so an existing host can
    /// pass <c>(byte)colorType</c> straight through.
    /// </para>
    /// </summary>
    public interface IVerilogColorPalette
    {
        /// <summary>Maps a semantic category to its palette index.</summary>
        byte ColorIndex(VerilogColorType colorType);
    }

    /// <summary>
    /// Semantic colour categories the parser distinguishes.
    ///
    /// <para>
    /// The numeric values are the editor's palette slots and must not be
    /// renumbered: they are written into documents as raw bytes, so a change
    /// here would recolour every cached parse result.
    /// </para>
    /// </summary>
    public enum VerilogColorType : byte
    {
        Normal = 0,
        Inactivated = 1,
        Register = 3,
        Keyword = 4,
        Comment = 5,
        Identifier = 6,
        Parameter = 7,
        Number = 8,
        Net = 9,
        HighLightedComment = 10,
        Variable = 11,
        CommentAnnotation = 12,
    }

    /// <summary>
    /// One autocomplete candidate produced by the parser.
    ///
    /// <para>
    /// Mirrors the editor's <c>AutocompleteItem</c> in the only fields the
    /// parser actually fills in: the label, the palette index used to colour
    /// it, and an optional icon path. Hosts turn this into their own
    /// presentation object.
    /// </para>
    /// </summary>
    public interface IVerilogAutocompleteItem
    {
        /// <summary>Text inserted when the candidate is accepted.</summary>
        string Label { get; }

        /// <summary>Palette index for the label colour.</summary>
        byte ColorIndex { get; }

        /// <summary>Optional icon asset path (host-specific), or null.</summary>
        string? IconPath { get; }
    }

    /// <summary>
    /// One hint line shown at the caret (e.g. a port signature).
    ///
    /// <para>
    /// The editor renders these as a coloured popup; the language server
    /// forwards them as signature help. The parser only produces the text and
    /// the palette index, so neither host leaks into it.
    /// </para>
    /// </summary>
    public interface IVerilogPopupItem
    {
        /// <summary>Plain text of the hint.</summary>
        string Text { get; }

        /// <summary>Palette index for the text colour.</summary>
        byte ColorIndex { get; }
    }

    // NOTE: the project / `include` resolution surface is intentionally NOT
    // abstracted yet.
    //
    // `WordScanner.parseInclude` resolves an include target through
    // `CodeEditor2.Data.Project.FindItems` / `SearchFile` and then builds the
    // header instance through `Data.VerilogHeaderInstance.Create`, which takes
    // `CodeEditor2.Data.Item` / `TextFile` and registers into the parsed
    // document's IncludeFiles table. A guessed interface here would not match
    // that shape, so it is left for the stage that actually rewrites the
    // include path (stage 3), where the real call sites are in scope and can
    // validate the design.
}
