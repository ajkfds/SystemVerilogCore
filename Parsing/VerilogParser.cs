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
    /// Drives one parse of a source file and produces a
    /// <see cref="Verilog.ParsedDocument"/>.
    ///
    /// <para>
    /// <b>Why this lives here.</b> The parse itself was always UI-free: it
    /// tokenises a document contract and walks <c>Root.ParseCreateAsync</c>.
    /// Only the surrounding plumbing named the editor (a parser base class in the
    /// editor assembly, a concrete <c>CodeDocument</c>, a concrete
    /// <c>VerilogFile</c>). This type is that plumbing, written against the
    /// contracts, so a host without a UI stack - the language server, the tests -
    /// can parse without referencing the editor at all.
    /// </para>
    ///
    /// <para>
    /// The editor keeps its own <c>Parser.VerilogParser</c> (derived from the
    /// editor's <c>DocumentParser</c>) because it needs the colour table and the
    /// acceptance path; that one delegates the parse work here in spirit, but the
    /// two are independent types. Hosts that only need a parse result use this
    /// one.
    /// </para>
    /// </summary>
    public sealed class VerilogParser
    {
        private readonly ParsedDocument parsedDocument;
        private WordScanner word = null!;

        /// <summary>
        /// Prepares a parse of <paramref name="file"/>'s whole text.
        /// </summary>
        /// <param name="file">The file being parsed, as the UI-free contract.</param>
        /// <param name="document">
        /// The buffer to tokenise. The caller owns it, so a host that wants the
        /// colours back keeps its own instance; a host that does not may pass a
        /// <see cref="PlainTextDocument"/>.
        /// </param>
        /// <param name="parseMode">Why the document is being parsed.</param>
        public VerilogParser(
            IVerilogSourceFile file,
            IVerilogTextDocument document,
            VerilogParseMode parseMode)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (document == null) throw new ArgumentNullException(nameof(document));

            this.file = file;

            // The buffer is passed in: an in-memory file has no document of its
            // own, so the result must not try to read one off the file.
            string key = Verilog.ParsedDocument.KeyGenerator(file, null, null);
            parsedDocument = new Verilog.ParsedDocument(file, key, null, parseMode, document)
            {
                SystemVerilog = file.IsSystemVerilog,
            };

            word = new WordScanner(document, parsedDocument, parsedDocument.SystemVerilog);
        }

        /// <summary>
        /// Prepares a parse of one parameter-overridden instance of a module in
        /// <paramref name="file"/>.
        ///
        /// <para>
        /// A module instantiated as <c>child #(.WIDTH(4)) u(...)</c> is parsed
        /// again with those overrides applied, under a key derived from the module
        /// name and the overrides. The result is used to check that instance's
        /// port widths and <c>generate</c> branches, so it must not be confused
        /// with the file's default-parameter parse.
        /// </para>
        /// </summary>
        public VerilogParser(
            IVerilogSourceFile file,
            IVerilogTextDocument document,
            string moduleName,
            Dictionary<string, Verilog.Expressions.Expression> parameterOverrides,
            VerilogParseMode parseMode)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrEmpty(moduleName)) throw new ArgumentNullException(nameof(moduleName));

            this.file = file;

            string key = Verilog.ParsedDocument.KeyGenerator(file, moduleName, parameterOverrides);
            parsedDocument = new Verilog.ParsedDocument(file, key, null, parseMode, document)
            {
                SystemVerilog = file.IsSystemVerilog,
                Instance = true,
                ParameterOverrides = parameterOverrides,
                TargetBuildingBlockName = moduleName,
            };

            word = new WordScanner(document, parsedDocument, parsedDocument.SystemVerilog);
        }

        private readonly IVerilogSourceFile file;

        /// <summary>The parse result being filled in.</summary>
        public Verilog.ParsedDocument VerilogParsedDocument => parsedDocument;

        /// <summary>The parse result, as the contract.</summary>
        public IParsedDocument ParsedDocument => parsedDocument;

        /// <summary>
        /// Runs the parse. The result is available from
        /// <see cref="VerilogParsedDocument"/> afterwards.
        /// </summary>
        public async Task ParseAsync(CancellationToken? cancellationToken = null)
        {
            word.GetFirst();
            word.CancellationToken = cancellationToken;

            await Root.ParseCreateAsync(word, parsedDocument, file);
        }

        /// <summary>
        /// Registers the file's root-level declarations into the project's name
        /// spaces.
        ///
        /// <para>
        /// The editor does this while accepting a parse result; a host that drives
        /// the parser itself has to do it too, or cross-object lookups (a class
        /// member, a package type) cannot resolve and every such reference reports
        /// "unfound object". Runs after the parse, once the building blocks exist.
        /// </para>
        /// </summary>
        public void RegisterRootElements()
        {
            if (parsedDocument.Root == null) return;
            if (file.ProjectSettings is not Verilog.IVerilogParserSettings settings) return;

            foreach (INamedElement element in parsedDocument.Root.NamedElements.Values)
            {
                if (element is Package package)
                {
                    settings.PackageNameSpace.Register(package.Name, package, file as Verilog.IVerilogNameSpaceFile);
                    continue;
                }

                if (element is Class class_)
                {
                    settings.UnitNameSpace.Register(class_.Name, class_, file as Verilog.IVerilogNameSpaceFile);
                    continue;
                }

                if (element is InterfaceClass interfaceClass)
                {
                    settings.UnitNameSpace.Register(interfaceClass.Name, interfaceClass, file as Verilog.IVerilogNameSpaceFile);
                    continue;
                }

                if (element is BuildingBlock buildingBlock)
                {
                    settings.DefinitionNameSpace.Register(buildingBlock.Name, buildingBlock, file as Verilog.IVerilogNameSpaceFile);
                    continue;
                }

                settings.UnitNameSpace.Register(element.Name, element, file as Verilog.IVerilogNameSpaceFile);
            }
        }
    }
}
