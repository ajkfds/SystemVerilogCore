using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;

namespace SystemVerilogCore.Snippets
{
    /// <summary>
    /// The "Invert Ports" tool: flips <c>input</c> / <c>output</c> and the
    /// <c>_I</c> / <c>_O</c> name suffix of every selected port declaration,
    /// preserving the original whitespace and comments.
    /// </summary>
    public static class PortInvertSnippetBuilder
    {
        /// <summary>Builds the snippet for the selected range.</summary>
        public static void Build(
            ParsedDocument parsedDocument,
            int selectionStart,
            int selectionEnd,
            SnippetProvider.SnippetResult result)
        {
            IVerilogTextDocument? doc = parsedDocument.VerilogDocument;
            if (doc == null) return;
            if (selectionStart < 0) selectionStart = 0;
            if (selectionEnd > doc.Length) selectionEnd = doc.Length;
            if (selectionEnd <= selectionStart)
            {
                result.Message = "select the port declarations to invert first";
                return;
            }

            string selected = doc.CreateString(selectionStart, selectionEnd - selectionStart);
            string[] lines = selected.Split('\n');
            List<string> processed = new List<string>(lines.Length);
            foreach (string line in lines)
            {
                processed.Add(SnippetBuilder.InvertPortLine(line));
            }
            string text = string.Join("\n", processed);

            result.Snippets.Add(new SnippetProvider.SnippetEntry
            {
                Label = "PortInvert",
                Detail = "invert the selected port declarations",
                Kind = SnippetProvider.SnippetKind.Replace,
                Start = selectionStart,
                End = selectionEnd,
                SnippetText = text,
                // Literal replacement: the inverted declarations contain no tab
                // stops, so both hosts insert the same text.
                PlainText = text,
            });
        }
    }
}
