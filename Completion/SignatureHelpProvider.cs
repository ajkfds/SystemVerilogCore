using System;
using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;

namespace SystemVerilogCore.Completion
{
    /// <summary>
    /// UI-free signature-help provider for hosts that do not drive the editor.
    ///
    /// <para>
    /// The parser already computes the "argument hint" for the caret position
    /// (the port / parameter label of the argument being typed) and stores it in
    /// <see cref="CompletionContext.CarletPopupItems"/> during the partial parse.
    /// This provider drives the same partial parse and converts those popup items
    /// into plain signature strings, so a host without an Avalonia dispatcher
    /// (the language server, the tests) can serve
    /// <c>textDocument/signatureHelp</c>.
    /// </para>
    ///
    /// <para>
    /// The parser does not model a full call signature (name + all parameters +
    /// active parameter index) at the caret, so this reports the single hint the
    /// editor would show as one signature with no parameter list. That keeps the
    /// LSP result honest: it is exactly what the editor displays.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>pluginVerilog.CoreBridge.SignatureHelpAdapter</c>) and took the plugin's
    /// concrete <c>VerilogFile</c>, so the language server had to reference the
    /// editor stack to answer a signature-help request.
    /// </para>
    /// </summary>
    public static class SignatureHelpProvider
    {
        /// <summary>One signature hint (the label the editor would pop up).</summary>
        public sealed class SignatureEntry
        {
            /// <summary>The hint text.</summary>
            public string Label { get; init; } = "";
        }

        /// <summary>
        /// Returns the signature hints at the given index (caret position), or
        /// null when signature help is unavailable (no parse result / out of
        /// range index / no hint at the caret).
        /// </summary>
        public static IReadOnlyList<SignatureEntry>? GetSignatures(
            IVerilogSourceFile file,
            ParsedDocument parsedDocument,
            int index)
        {
            if (file == null || parsedDocument == null) return null;
            if (index < 0) return null;

            // Same buffer preparation as CompletionProvider (one implementation,
            // so the two cannot drift).
            IVerilogTextDocument? codeDocument = CompletionProvider.PrepareCompletionBuffer(parsedDocument, file);
            if (codeDocument == null) return null;
            if (index > codeDocument.Length) return null;

            CompletionContext context =
                new CompletionContext(file, parsedDocument, index, codeDocument);
            if (context.AutoCompleteItems.Count == 0) context.AppendAll();

            List<SignatureEntry> entries = new List<SignatureEntry>();
            HashSet<string> seen = new HashSet<string>();
            foreach (IVerilogPopupItem item in context.CarletPopupItems)
            {
                if (item == null) continue;
                string label = item.CreateString();
                if (string.IsNullOrEmpty(label)) continue;
                if (!seen.Add(label)) continue;
                entries.Add(new SignatureEntry { Label = label });
            }
            return entries.Count == 0 ? null : entries;
        }
    }
}
