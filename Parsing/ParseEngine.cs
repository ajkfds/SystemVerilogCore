using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;
using SystemVerilogCore.Verilog.BuildingBlocks;

namespace SystemVerilogCore.Parsing
{
    /// <summary>
    /// Entry point for hosts that parse without the editor: the language server
    /// and the tests.
    ///
    /// <para>
    /// <b>Why this lives here.</b> It used to sit in the editor plugin
    /// (<c>CoreBridge.ParseEngine</c>), which meant the language server had to
    /// reference the plugin - and through it the whole Avalonia stack - just to
    /// parse. Nothing in the parse needs the UI; only the plumbing did. Moving it
    /// here is what lets a host reference <c>SystemVerilogCore</c> alone.
    /// </para>
    ///
    /// <para>
    /// The editor keeps its own parse path (<c>Parser.VerilogParser</c> +
    /// <c>TextFile.AcceptParsedDocumentAsync</c>) because it has to copy colour
    /// marks and refresh the tree. This engine deliberately does neither.
    /// </para>
    /// </summary>
    public static class ParseEngine
    {
        /// <summary>
        /// Parses <paramref name="text"/> and returns the result, or null when
        /// nothing could be parsed.
        ///
        /// <para>
        /// Parses up to twice. The first pass registers the file's own building
        /// blocks; a forward reference such as <c>-&gt;&gt; top.e</c> where
        /// <c>top</c> is declared later in the same file cannot resolve until
        /// those names exist, so a second pass runs when the first reported
        /// errors (the same effect as the editor's multi-pass hierarchy parse).
        /// </para>
        /// </summary>
        /// <param name="text">The file text to parse.</param>
        /// <param name="file">The file, as the UI-free source-file contract.</param>
        /// <param name="cancellationToken">Cancellation token, or null.</param>
        public static async Task<Verilog.ParsedDocument?> ParseSystemVerilogAsync(
            string text,
            IVerilogSourceFile file,
            CancellationToken? cancellationToken = null)
        {
            if (text == null) return null;
            if (file == null) return null;

            Verilog.ParsedDocument? parsedDocument =
                await ParseOnceAsync(text, file, cancellationToken);
            if (parsedDocument == null) return null;

            if (parsedDocument.ErrorCount > 0)
            {
                Verilog.ParsedDocument? second =
                    await ParseOnceAsync(text, file, cancellationToken);
                if (second != null) parsedDocument = second;
            }

            return parsedDocument;
        }

        /// <summary>
        /// Parses <paramref name="text"/> as one parameter-overridden instance of
        /// <paramref name="moduleName"/>.
        ///
        /// <para>
        /// A plain file parse uses the declared default parameter values, so a
        /// module instantiated with <c>#(.P_WIDTH(5))</c> would still be reported
        /// with its default. This gives a host the same result the editor shows
        /// when the instance is opened.
        /// </para>
        ///
        /// <para>
        /// Unlike <see cref="ParseSystemVerilogAsync"/> this does NOT register the
        /// parsed building block into the project name spaces: the file's plain
        /// parse already registered the default-parameter version, and
        /// overwriting it with one instance's override would corrupt the
        /// resolution of every other instance of the same module.
        /// </para>
        /// </summary>
        public static async Task<Verilog.ParsedDocument?> ParseInstanceAsync(
            string text,
            IVerilogSourceFile file,
            string moduleName,
            Dictionary<string, Verilog.Expressions.Expression> parameterOverrides,
            CancellationToken? cancellationToken = null)
        {
            if (text == null) return null;
            if (file == null) return null;
            if (string.IsNullOrEmpty(moduleName)) return null;

            IVerilogTextDocument document = CreateDocument(text, file);

            VerilogParser parser = new VerilogParser(
                file, document, moduleName, parameterOverrides, VerilogParseMode.BackgroundParse);

            await parser.ParseAsync(cancellationToken ?? CancellationToken.None);

            Verilog.ParsedDocument parsed = parser.VerilogParsedDocument;

            // Record the instance result under its PARAMETER-QUALIFIED key, which
            // is what the editor reads an instance through
            // (InstanceTextFile.ParsedDocument -> GetInstancedParsedDocument).
            // Without this every instance read back null and the opened document
            // fell back to the plain file parse (declared parameter values).
            string instanceKey =
                Verilog.ParsedDocument.KeyGenerator(file, moduleName, parameterOverrides);
            (file as IVerilogSourceFileInstanceCache)?.RegisterInstanceParsedDocument(instanceKey, parsed);

            return parsed;
        }

        private static async Task<Verilog.ParsedDocument?> ParseOnceAsync(
            string text,
            IVerilogSourceFile file,
            CancellationToken? cancellationToken)
        {
            IVerilogTextDocument document = CreateDocument(text, file);

            VerilogParser parser = new VerilogParser(
                file, document, VerilogParseMode.BackgroundParse);

            await parser.ParseAsync(cancellationToken ?? CancellationToken.None);

            Verilog.ParsedDocument parsedDocument = parser.VerilogParsedDocument;

            parser.RegisterRootElements();

            return parsedDocument;
        }

        /// <summary>
        /// The buffer to tokenise: the host's own document when it has one (so the
        /// colour spans land where the host can read them), otherwise a plain
        /// buffer.
        /// </summary>
        private static IVerilogTextDocument CreateDocument(string text, IVerilogSourceFile file)
        {
            IVerilogTextDocument? existing = file.VerilogTextDocument;
            if (existing != null
                && string.Equals(existing.CreateString(), text, StringComparison.Ordinal))
            {
                return existing;
            }

            // The buffer reports the file as its owner: a parse drops a message
            // when the buffer belongs to no file, so an in-memory host would lose
            // every diagnostic otherwise.
            return new PlainTextDocument(text, file as IVerilogDocumentFile);
        }
    }
}
