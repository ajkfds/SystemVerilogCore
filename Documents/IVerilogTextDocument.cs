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

        /// <summary>
        /// Records a diagnostic mark (the squiggle the editor draws) over
        /// <paramref name="length"/> characters from <paramref name="index"/>,
        /// using the host's mark style entry <paramref name="markIndex"/>
        /// (0 = error, 1 = warning, ...).
        ///
        /// <para>
        /// The parser raises errors / warnings / notices / hints through
        /// <c>ParsedDocument.AddError</c> &amp; friends, which both record a
        /// message and ask the document to mark the offending span. Hosts that
        /// cannot draw marks (the language server reports diagnostics instead)
        /// may implement this as a no-op.
        /// </para>
        /// </summary>
        void SetMarkAt(int index, int length, byte markIndex);

        /// <summary>
        /// The file this document belongs to, or null for a throw-away
        /// document (a macro expansion buffer, a snapshot taken for an
        /// include). Used by diagnostics to record which file a message came
        /// from.
        /// </summary>
        IVerilogDocumentFile? File { get; }

        /// <summary>
        /// The zero-based line containing <paramref name="index"/>. Used to
        /// stamp diagnostics with a line number.
        /// </summary>
        int GetLineAt(int index);

        /// <summary>
        /// Start offset of the zero-based <paramref name="line"/>.
        ///
        /// <para>
        /// The counterpart of <see cref="GetLineAt"/>, and 0-based for the same
        /// reason: a host that reports positions in LSP / zero-based terms can
        /// convert an offset to (line, character) without knowing whether the
        /// underlying editor counts lines from 0 or 1. The host implementation
        /// owns that conversion.
        /// </para>
        /// </summary>
        int GetLineStartIndex(int line);
    }

    /// <summary>
    /// The identity of the file a document was created from.
    ///
    /// <para>
    /// Only what the parser needs for diagnostics and cross-file lookups: an
    /// id, a project-relative path and a display name. The editor's
    /// <c>Data.IVerilogRelatedFile</c> and the language server's in-memory file
    /// both satisfy this shape, which is why the parser can talk to either
    /// without referencing the editor's data types.
    /// </para>
    /// </summary>
    public interface IVerilogDocumentFile
    {
        /// <summary>Unique id of the file (host-defined).</summary>
        string Id { get; }

        /// <summary>Project-relative path, '/'-separated.</summary>
        string RelativePath { get; }

        /// <summary>Display name (usually the file name).</summary>
        string Name { get; }
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

    // NOTE: the caret-hint ("popup") surface is intentionally NOT abstracted.
    //
    // The editor's `PopupItem` derives from `AjkAvaloniaLibs.Controls.ColorLabel`,
    // i.e. it is an ordered list of individually coloured text/image spans, and
    // the parser builds those spans directly (`Port.GetLabel()` returns a
    // ColorLabel). A `{ Text, ColorIndex }` pair cannot represent that, so an
    // interface of this shape would be a lie. It will be designed in the stage
    // that rewrites the hint call sites, where the real shape is in scope.

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
