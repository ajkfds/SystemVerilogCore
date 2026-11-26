namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A plain autocomplete candidate the parser produces on its own.
    ///
    /// <para>
    /// The parser builds completion candidates as it walks the tree (keywords,
    /// macros, system tasks / functions, declared elements, members). Those
    /// candidates used to be the editor's <c>AutoCompleteItem</c>, which is why
    /// the completion path kept a reference to the editor assembly. This type
    /// carries exactly the data the parser decides - label, palette index, icon
    /// path and kind - so a host can wrap it in its own presentation type.
    /// </para>
    ///
    /// <para>
    /// <see cref="InsertText"/> defaults to <see cref="Label"/>: a candidate that
    /// expands to something else (a module name to a whole instantiation, a
    /// block keyword to its skeleton) is created by the host, which is the side
    /// that knows the expansion.
    /// </para>
    /// </summary>
    public sealed class VerilogAutocompleteCandidate : IVerilogAutocompleteItem
    {
        public VerilogAutocompleteCandidate(
            string label,
            byte colorIndex,
            SystemVerilogAutocompleteItemKind kind,
            string? iconPath = null)
        {
            Label = label;
            ColorIndex = colorIndex;
            Kind = kind;
            IconPath = iconPath;
        }

        /// <inheritdoc />
        public string Label { get; }

        /// <inheritdoc />
        public string InsertText => Label;

        /// <inheritdoc />
        public byte ColorIndex { get; }

        /// <inheritdoc />
        public string? IconPath { get; }

        /// <inheritdoc />
        public SystemVerilogAutocompleteItemKind Kind { get; }
    }
}
