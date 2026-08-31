namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A position in a document expressed as zero-based line/column and an
    /// absolute character index. The absolute index is what the parser
    /// produces; the line/column is what most UI/LSP consumers need.
    /// </summary>
    public readonly struct SystemVerilogPosition
    {
        public int Line { get; }
        public int Column { get; }
        public int Index { get; }

        public SystemVerilogPosition(int line, int column, int index)
        {
            Line = line;
            Column = column;
            Index = index;
        }

        public override string ToString()
        {
            return $"({Line},{Column})@{Index}";
        }
    }
}
