using System;
using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A throw-away, string-backed document that implements the parser's
    /// document contract.
    ///
    /// <para>
    /// The parser sometimes tokenizes text it did not get from a file: a slice
    /// of the buffer for an autocomplete partial parse, a line of hint text, a
    /// macro expansion body. Those buffers never need colours, marks or fold
    /// regions - the result is read immediately and discarded - so the write
    /// methods are no-ops here. Using this instead of a host document keeps such
    /// sites free of the editor's document type.
    /// </para>
    ///
    /// <para>
    /// Lines are indexed lazily and cached; the text is immutable after
    /// construction.
    /// </para>
    /// </summary>
    public sealed class PlainTextDocument : IVerilogTextDocument
    {
        private readonly string text;
        private List<int>? lineStarts;

        /// <summary>Creates a document over <paramref name="text"/>.</summary>
        public PlainTextDocument(string text)
        {
            this.text = text ?? "";
        }

        /// <inheritdoc />
        public int Length => text.Length;

        /// <inheritdoc />
        public char GetCharAt(int index)
        {
            if (index < 0 || index >= text.Length) return '\0';
            return text[index];
        }

        /// <inheritdoc />
        public string CreateString(int startIndex, int length)
        {
            if (length <= 0) return "";
            if (startIndex < 0) startIndex = 0;
            if (startIndex >= text.Length) return "";
            if (startIndex + length > text.Length) length = text.Length - startIndex;
            return text.Substring(startIndex, length);
        }

        /// <inheritdoc />
        public string CreateString() => text;

        /// <summary>
        /// No-op: these buffers are never displayed, so there is no colour table
        /// to fill.
        /// </summary>
        public void SetColorAt(int index, byte colorIndex, int length)
        {
        }

        /// <summary>
        /// A fresh plain-text buffer holding the same text.
        ///
        /// <para>
        /// This document is already a throw-away buffer (nothing displays it), so
        /// a snapshot is just another plain buffer. It is only ever tokenised and
        /// coloured, so dropping the colour spans is harmless.
        /// </para>
        /// </summary>
        public IVerilogTextDocument CreateSnapshot()
        {
            return new PlainTextDocument(text);
        }

        /// <summary>No-op: these buffers are never displayed.</summary>
        public void SetMarkAt(int index, int length, byte markIndex)
        {
        }

        /// <summary>Null: a throw-away buffer belongs to no file.</summary>
        public IVerilogDocumentFile? File => null;

        /// <inheritdoc />
        public int GetLineAt(int index)
        {
            List<int> starts = GetLineStarts();
            if (index < 0) return 0;
            int found = starts.BinarySearch(index);
            if (found >= 0) return found;
            // BinarySearch returns the bitwise complement of the next larger
            // element: the line start after the index, so the line is one before.
            return (found ^ -1) - 1;
        }

        /// <inheritdoc />
        public int GetLineStartIndex(int line)
        {
            List<int> starts = GetLineStarts();
            if (line < 0) return 0;
            if (line >= starts.Count) return text.Length;
            return starts[line];
        }

        /// <inheritdoc />
        public int GetLineLength(int line)
        {
            List<int> starts = GetLineStarts();
            if (line < 0 || line >= starts.Count) return 0;
            int start = starts[line];
            int end = line + 1 < starts.Count ? starts[line + 1] : text.Length;
            int length = end - start;
            // Exclude the terminator, matching the host documents.
            if (length > 0 && text[end - 1] == '\n') length--;
            if (length > 0 && text[end - 1] == '\r') length--;
            return length;
        }

        /// <summary>No-op: these buffers are never displayed.</summary>
        public void AppendBlock(int startIndex, int lastIndex)
        {
        }

        /// <summary>No-op: these buffers are never displayed.</summary>
        public void AppendBlock(int startIndex, int lastIndex, string name, bool defaultClose)
        {
        }

        /// <summary>
        /// Start offsets of every line, with '\n' as the terminator. Built on
        /// first use and cached; the text never changes.
        /// </summary>
        private List<int> GetLineStarts()
        {
            if (lineStarts != null) return lineStarts;

            List<int> starts = new List<int> { 0 };
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n' && i + 1 <= text.Length - 1) starts.Add(i + 1);
            }
            lineStarts = starts;
            return starts;
        }
    }
}
