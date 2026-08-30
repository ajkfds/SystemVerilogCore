namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A half-open character range within a document. Used for diagnostics,
    /// definition locations and similar LSP-style operations. Lines and
    /// columns are 0-based to match the LSP convention.
    /// </summary>
    public readonly struct SystemVerilogRange
    {
        public int StartIndex { get; }
        public int Length { get; }

        public int EndIndex => StartIndex + Length;

        public SystemVerilogRange(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        public bool Contains(int index)
        {
            return index >= StartIndex && index < EndIndex;
        }

        public override string ToString()
        {
            return $"[{StartIndex}..{EndIndex})";
        }
    }
}
