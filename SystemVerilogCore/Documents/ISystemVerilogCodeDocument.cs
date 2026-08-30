using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Abstraction over the underlying text document. The host (IDE editor or
    /// LSP) provides an implementation; the SystemVerilog core only depends
    /// on the read-only surface needed for parsing and diagnostics.
    /// </summary>
    public interface ISystemVerilogCodeDocument
    {
        /// <summary>Total length of the document in characters.</summary>
        int Length { get; }

        /// <summary>Number of lines in the document.</summary>
        int LineCount { get; }

        /// <summary>Returns the character at <paramref name="index"/>.</summary>
        char GetCharAt(int index);

        /// <summary>Returns a substring of the document.</summary>
        string GetText(int startIndex, int length);

        /// <summary>Returns the entire text of the document.</summary>
        string GetText();

        /// <summary>Returns the entire text of the given line.</summary>
        string GetLineText(int line);

        /// <summary>Returns the zero-based line containing the given index.</summary>
        int GetLineAt(int index);

        /// <summary>Returns the start index of the given line.</summary>
        int GetLineStartIndex(int line);

        /// <summary>Returns the length of the given line (without line terminator).</summary>
        int GetLineLength(int line);

        /// <summary>
        /// Locates the word that contains the given character index. Returns
        /// false if the index is not within a word.
        /// </summary>
        bool TryGetWord(int index, out int wordStart, out int wordLength);

        /// <summary>
        /// Snapshot version of the underlying text. Bumped whenever the
        /// document is mutated so that consumers can detect stale state.
        /// </summary>
        ulong Version { get; }
    }
}
