using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;
using SystemVerilogCore.Verilog.BuildingBlocks;

namespace SystemVerilogCore.Snippets
{
    /// <summary>
    /// The "Insert Module Instance" tool: insert a full instantiation of
    /// <c>moduleName</c> (parameter overrides plus one named connection per port,
    /// each an empty tab stop) at the caret.
    ///
    /// <para>
    /// Called twice: without a module name it lists the project's modules as
    /// candidates (the host shows a picker and calls again with the chosen name).
    /// </para>
    /// </summary>
    public static class ModuleInstanceSnippetBuilder
    {
        /// <summary>
        /// Builds the snippet (or the candidate list) for the caret at
        /// <paramref name="index"/>.
        /// </summary>
        /// <param name="file">The file being edited (its project settings are used).</param>
        /// <param name="parsedDocument">Parse result of <paramref name="file"/>.</param>
        /// <param name="index">Caret offset in the document.</param>
        /// <param name="moduleName">
        /// The module to instantiate. When null / empty, the candidate module
        /// names are returned so the host can show a picker.
        /// </param>
        /// <param name="result">Receives the snippet or the candidates.</param>
        public static void Build(
            IVerilogSourceFile file,
            ParsedDocument parsedDocument,
            int index,
            string? moduleName,
            SnippetProvider.SnippetResult result)
        {
            IVerilogParserSettings? settings = file.ProjectSettings as IVerilogParserSettings;
            if (settings == null)
            {
                result.Message = "the project has no Verilog property";
                return;
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                // no module chosen yet: hand the candidates to the host
                foreach (string name in settings.DefinitionNameSpace.GetNameList(
                             x => x is Module))
                {
                    result.Candidates.Add(name);
                }
                if (result.Candidates.Count == 0) result.Message = "no module found in the project";
                return;
            }

            Module? module = SnippetBuilder.ResolveModule(settings, moduleName);
            if (module == null)
            {
                result.Message = "module not found: " + moduleName;
                return;
            }

            string instanceName = SnippetBuilder.MakeUniqueInstanceName(parsedDocument, index, moduleName);
            string indent = SnippetBuilder.GetIndent(parsedDocument, index);
            SnippetTemplate template =
                SnippetBuilder.BuildInstantiationTemplate(module, instanceName, indent, null);

            List<SnippetField> fields = new();
            string plainText = template.ToPlainText(fields);

            result.Snippets.Add(new SnippetProvider.SnippetEntry
            {
                Label = moduleName,
                Detail = "instantiate " + moduleName,
                Kind = SnippetProvider.SnippetKind.Insert,
                Start = index,
                End = index,
                SnippetText = template.ToVscodeSnippet(),
                PlainText = plainText,
                Fields = fields,
            });
        }
    }
}
