namespace SystemVerilogCore.Parsing
{
    /// <summary>
    /// Why a document is being parsed.
    ///
    /// <para>
    /// The parser changes behaviour per mode: a full load may defer work that an
    /// interactive edit needs immediately, and a single-building-block hierarchy
    /// parse only resolves the block it was asked about. The parser therefore
    /// has to know the mode, but it must not depend on the editor's
    /// <c>DocumentParser.ParseModeEnum</c>.
    /// </para>
    ///
    /// <para>
    /// The editor's enum keeps the same values and order, and
    /// <c>DocumentParser.ParseModeEnum</c> is now an alias of this type, so both
    /// hosts share one definition.
    /// </para>
    /// </summary>
    public enum VerilogParseMode
    {
        /// <summary>Full load parse of a file opened from disk.</summary>
        LoadParse,

        /// <summary>Background parse of a file the editor has not focused.</summary>
        BackgroundParse,

        /// <summary>Interactive parse while the user is editing.</summary>
        EditParse,

        /// <summary>Parse a single building block for the hierarchy view.</summary>
        SingleHierParse,
    }
}
