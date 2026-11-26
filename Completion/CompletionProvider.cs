using System;
using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Snippets;
using SystemVerilogCore.Verilog;

namespace SystemVerilogCore.Completion
{
    /// <summary>
    /// UI-free completion adapter for hosts that do not drive the editor.
    ///
    /// <para>
    /// Drives the parser's <see cref="Verilog.CompletionContext"/> (partial parse
    /// based autocomplete) on an already-parsed document and converts the result
    /// into plain (text, kind) entries, so a host without an Avalonia dispatcher
    /// (the language server, the tests) can serve <c>textDocument/completion</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.CompletionAdapter</c>) and took the plugin's
    /// concrete <c>VerilogFile</c>, so the language server had to reference the
    /// editor stack to answer a completion request. Both inputs are contracts now
    /// (<see cref="IVerilogSourceFile"/> and the parse result), and the snippet
    /// body comes from <see cref="SnippetProvider"/>.
    /// </para>
    /// </summary>
    public static class CompletionProvider
    {
        /// <summary>One completion candidate, in the shape the LSP layer serialises.</summary>
        public sealed class CompletionEntry
        {
            /// <summary>Label shown in the completion list.</summary>
            public string Text { get; init; } = "";

            /// <summary>LSP CompletionItemKind value.</summary>
            public int Kind { get; init; }

            /// <summary>Short description (the candidate kind).</summary>
            public string Detail { get; init; } = "";

            /// <summary>
            /// Text actually inserted when the entry is accepted. Null means
            /// "insert <see cref="Text"/> as-is" (plain text).
            /// </summary>
            public string? InsertText { get; init; }

            /// <summary>
            /// LSP InsertTextFormat: 1 = PlainText, 2 = Snippet.
            /// Only meaningful when <see cref="InsertText"/> is set.
            /// </summary>
            public int InsertTextFormat { get; init; } = 1;
        }

        /// <summary>
        /// Returns completion items at the given index (caret position),
        /// or null when completion is unavailable (no parse result / out of
        /// range index).
        /// </summary>
        public static IReadOnlyList<CompletionEntry>? GetCompletionItems(
            IVerilogSourceFile file,
            ParsedDocument parsedDocument,
            int index)
        {
            if (file == null || parsedDocument == null) return null;
            if (index < 0) return null;

            // The partial parse reads the source text from the file's buffer,
            // which a UI-less host has not loaded yet.
            IVerilogTextDocument? codeDocument = PrepareCompletionBuffer(parsedDocument, file);
            if (codeDocument == null) return null;
            if (index > codeDocument.Length) return null;

            // runs the real CompletionContext (partial parse of the region
            // around the caret, UI-free)
            Verilog.CompletionContext context =
                new Verilog.CompletionContext(file, parsedDocument, index, codeDocument);
            if (context.AutoCompleteItems.Count == 0) context.AppendAll();

            List<CompletionEntry> entries = new List<CompletionEntry>();
            HashSet<string> seen = new HashSet<string>();
            foreach (IVerilogAutocompleteItem contractItem in context.AutoCompleteItems)
            {
                if (contractItem == null) continue;
                string text = contractItem.Label;
                if (string.IsNullOrEmpty(text)) continue;
                if (!seen.Add(text)) continue; // reject duplicated entries

                // LSP CompletionItemKind: 3=Function, 6=Variable, 7=Class,
                // 9=Module, 14=Keyword, 15=Snippet.
                int kind = contractItem.Kind switch
                {
                    SystemVerilogAutocompleteItemKind.Function => 3,
                    SystemVerilogAutocompleteItemKind.Task => 3,
                    SystemVerilogAutocompleteItemKind.Variable => 6,
                    SystemVerilogAutocompleteItemKind.Net => 6,
                    SystemVerilogAutocompleteItemKind.Parameter => 6,
                    SystemVerilogAutocompleteItemKind.LocalParameter => 6,
                    SystemVerilogAutocompleteItemKind.Port => 6,
                    SystemVerilogAutocompleteItemKind.Module => 9,
                    SystemVerilogAutocompleteItemKind.Interface => 9,
                    SystemVerilogAutocompleteItemKind.Package => 9,
                    SystemVerilogAutocompleteItemKind.Class => 7,
                    SystemVerilogAutocompleteItemKind.Typedef => 7,
                    SystemVerilogAutocompleteItemKind.Macro => 14,
                    SystemVerilogAutocompleteItemKind.Snippet => 15,
                    _ => 14,
                };
                string detail = contractItem.Kind.ToString();

                // A module-name candidate expands to a FULL instantiation
                // (parameter overrides + one named connection per port) when
                // accepted, not just the module name. Report it with that text
                // as the snippet body, otherwise the VS Code host inserts the
                // bare name - the "module name completion is only a keyword
                // completion" symptom.
                //
                // Recognised by asking the snippet code: a module instance
                // request for that name produces an instantiation. The
                // provider cannot test the candidate's type (it lives in the
                // editor assembly), and the label is the module name.
                if (contractItem.Kind == SystemVerilogAutocompleteItemKind.Snippet
                    && IsModuleInstanceCandidate(file, parsedDocument, index, text))
                {
                    entries.Add(new CompletionEntry
                    {
                        Text = text,
                        Kind = 9, // LSP CompletionItemKind.Module
                        Detail = "module",
                        InsertText = BuildModuleInstanceSnippet(file, parsedDocument, index, text),
                        InsertTextFormat = 2, // Snippet
                    });
                    continue;
                }

                CompletionEntry entry = new CompletionEntry { Text = text, Kind = kind, Detail = detail };

                // Block keywords expand to a full block (begin ... end etc.) as a
                // VS Code snippet, mirroring the editor's block snippets.
                // Without this the LSP host only inserts the bare keyword.
                string? snippet = BuildBlockSnippet(text, codeDocument, index);
                if (snippet != null)
                {
                    entry = new CompletionEntry
                    {
                        Text = text,
                        Kind = kind,
                        Detail = detail,
                        InsertText = snippet,
                        InsertTextFormat = 2, // Snippet
                    };
                }

                entries.Add(entry);
            }
            return entries;
        }

        /// <summary>
        /// True when the candidate is the host's "expand a module name into an
        /// instantiation" item: a module of the project is called
        /// <paramref name="label"/> and the snippet code produces an
        /// instantiation for it.
        /// </summary>
        /// <remarks>
        /// The editor's candidate is a snippet item whose label is the module
        /// name. Its type lives in the editor assembly, so the type cannot be
        /// tested here; the module name can. A keyword that happens to share a
        /// module's name would be reported as a module candidate, but then the
        /// snippet body is the instantiation - which is what accepting the row
        /// does in the editor as well.
        /// </remarks>
        private static bool IsModuleInstanceCandidate(
            IVerilogSourceFile file,
            ParsedDocument parsedDocument,
            int index,
            string label)
        {
            if (string.IsNullOrEmpty(label)) return false;
            IVerilogParserSettings? settings = file.ProjectSettings as IVerilogParserSettings;
            if (settings == null) return false;
            if (SnippetBuilder.ResolveModule(settings, label) == null) return false;

            SnippetProvider.SnippetResult probe = SnippetProvider.GetSnippets(
                file, parsedDocument, index,
                SnippetProvider.SnippetName.ModuleInstance, label, 0, 0);
            return probe.Snippets.Count > 0;
        }

        /// <summary>
        /// Builds the snippet body for a module-name completion candidate: the
        /// full instantiation the editor inserts when the candidate is accepted.
        /// Falls back to the bare module name when the module cannot be resolved.
        /// </summary>
        private static string BuildModuleInstanceSnippet(
            IVerilogSourceFile file,
            ParsedDocument parsedDocument,
            int index,
            string moduleName)
        {
            try
            {
                SnippetProvider.SnippetResult result = SnippetProvider.GetSnippets(
                    file,
                    parsedDocument,
                    index,
                    SnippetProvider.SnippetName.ModuleInstance,
                    moduleName,
                    0,
                    0);
                foreach (SnippetProvider.SnippetEntry entry in result.Snippets)
                {
                    if (!string.IsNullOrEmpty(entry.SnippetText)) return entry.SnippetText;
                }
            }
            catch (Exception)
            {
                // never fail completion over the snippet body
            }
            return moduleName;
        }

        /// <summary>
        /// Builds a VS Code snippet body for block-opening keywords, or null when
        /// the keyword has no block expansion. The indentation of the line the
        /// caret sits on is reused for the closing keyword.
        /// </summary>
        private static string? BuildBlockSnippet(
            string keyword,
            IVerilogTextDocument codeDocument,
            int index)
        {
            string indent = GetLineIndent(codeDocument, index);
            string inner = indent + "\t";

            switch (keyword)
            {
                case "begin":
                    return "begin\n" + inner + "$0\n" + indent + "end";
                case "fork":
                    return "fork\n" + inner + "$0\n" + indent + "join";
                case "case":
                    return "case (${1})\n" + inner + "$0\n" + indent + "endcase";
                case "casex":
                    return "casex (${1})\n" + inner + "$0\n" + indent + "endcase";
                case "casez":
                    return "casez (${1})\n" + inner + "$0\n" + indent + "endcase";
                case "generate":
                    return "generate\n" + inner + "$0\n" + indent + "endgenerate";
                case "module":
                    return "module ${1:name};\n" + inner + "$0\n" + indent + "endmodule";
                case "interface":
                    return "interface ${1:name};\n" + inner + "$0\n" + indent + "endinterface";
                case "function":
                    return "function ${1:type} ${2:name};\n" + inner + "$0\n" + indent + "endfunction";
                case "task":
                    return "task ${1:name};\n" + inner + "$0\n" + indent + "endtask";
                default:
                    return null;
            }
        }

        /// <summary>Leading whitespace of the line containing <paramref name="index"/>.</summary>
        private static string GetLineIndent(IVerilogTextDocument codeDocument, int index)
        {
            if (index < 0) return "";
            if (index > codeDocument.Length) index = codeDocument.Length;

            int line = codeDocument.GetLineAt(index);
            int lineStart = codeDocument.GetLineStartIndex(line);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = lineStart; i < codeDocument.Length; i++)
            {
                char c = codeDocument.GetCharAt(i);
                if (c != ' ' && c != '\t') break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prepares the file's working buffer for a partial parse and returns it,
        /// or null when no buffer could be created.
        /// </summary>
        /// <remarks>
        /// A host that never loaded the file has no buffer; the parse result's
        /// own document is used as the fallback because the partial parse only
        /// needs the text (the colours of a throw-away buffer are discarded).
        /// </remarks>
        public static IVerilogTextDocument? PrepareCompletionBuffer(
            ParsedDocument parsedDocument,
            IVerilogSourceFile file)
        {
            IVerilogTextDocument? parsedDocumentBuffer = parsedDocument.VerilogDocument;
            if (parsedDocumentBuffer == null) return null;

            // Ask the host to make the buffer hold the parsed text. It is a host
            // operation (creating a document is a host type), and the buffer has
            // to be the file's own: the identifier-chain walk and the partial
            // parse both read it, and a throw-away buffer would not be the one
            // the host reports.
            file.PrepareVerilogTextDocument(parsedDocumentBuffer.CreateString());

            IVerilogTextDocument? codeDocument = file.VerilogTextDocument;
            if (codeDocument != null) return codeDocument;

            // The host has no buffer at all (a bare in-memory file): fall back to
            // one over the text. It reports the file, so diagnostics raised by
            // the partial parse are not dropped on the floor.
            return new PlainTextDocument(
                parsedDocumentBuffer.CreateString(),
                file as IVerilogDocumentFile);
        }
    }
}
