using System;
using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Snippets;
using SystemVerilogCore.Verilog;

namespace SystemVerilogCore.Completion
{
    /// <summary>
    /// UI-free formatting provider for hosts that do not drive the editor.
    ///
    /// <para>
    /// The editor's "autoFormat" tool rewrites the module instantiation under the
    /// caret (port connections aligned, one per line). That logic lives in
    /// <c>ModuleInstantiation.CreateString(indent)</c> and is UI-free; this
    /// provider drives it and returns plain text edits, so a host without an
    /// Avalonia dispatcher (the language server, the tests) can serve
    /// <c>textDocument/formatting</c>.
    /// </para>
    ///
    /// <para>
    /// Only module instantiations are reformatted (the same scope as the editor's
    /// tool). Everything else is left untouched, so the returned edit list is
    /// empty when the document has no instantiation to format.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.FormattingAdapter</c>) and read the indent
    /// through the plugin's concrete document, so the language server had to
    /// reference the editor stack to answer a format request. The indent comes
    /// from <see cref="SnippetBuilder.GetIndent"/> now, which reads the document
    /// contract.
    /// </para>
    /// </summary>
    public static class FormattingProvider
    {
        /// <summary>One replacement: [Start, End) in the document → NewText.</summary>
        public sealed class TextEditEntry
        {
            /// <summary>Document-absolute start offset.</summary>
            public int Start { get; init; }

            /// <summary>Document-absolute end offset, exclusive.</summary>
            public int End { get; init; }

            /// <summary>The replacement text.</summary>
            public string NewText { get; init; } = "";
        }

        /// <summary>
        /// Returns the edits that reformat every module instantiation in the
        /// document, or an empty list when there is nothing to format.
        /// </summary>
        public static IReadOnlyList<TextEditEntry> GetFormattingEdits(ParsedDocument? parsedDocument)
        {
            List<TextEditEntry> edits = new List<TextEditEntry>();
            if (parsedDocument?.Root == null) return edits;

            IVerilogTextDocument? codeDocument = parsedDocument.VerilogDocument;
            if (codeDocument == null) return edits;

            foreach (Verilog.BuildingBlocks.BuildingBlock block
                     in parsedDocument.Root.BuildingBlocks.Values)
            {
                foreach (Verilog.Items.IBuildingBlockInstantiation inst
                         in block.GetBuildingBlockInstantiations())
                {
                    if (inst is not Verilog.Items.ModuleInstantiation moduleInstantiation) continue;
                    if (moduleInstantiation.BeginIndexReference == null) continue;
                    if (moduleInstantiation.LastIndexReference == null) continue;

                    int start = SnippetBuilder.LastIndex(moduleInstantiation.BeginIndexReference);
                    int end = SnippetBuilder.LastIndex(moduleInstantiation.LastIndexReference);
                    if (start < 0 || end < start || end >= codeDocument.Length) continue;

                    string indent = SnippetBuilder.GetIndent(parsedDocument, start);
                    string? formatted = moduleInstantiation.CreateString(indent);
                    if (string.IsNullOrEmpty(formatted)) continue;

                    // CreateString returns the statement without the trailing
                    // semicolon; the source range already covers it, so append
                    // it when the original text ended with one.
                    string original = codeDocument.CreateString(start, end - start + 1);
                    if (original.TrimEnd().EndsWith(";", StringComparison.Ordinal)
                        && !formatted.TrimEnd().EndsWith(";", StringComparison.Ordinal))
                    {
                        formatted = formatted.TrimEnd() + ";";
                    }

                    if (string.Equals(original, formatted, StringComparison.Ordinal)) continue;

                    edits.Add(new TextEditEntry { Start = start, End = end + 1, NewText = formatted });
                }
            }
            return edits;
        }
    }
}
