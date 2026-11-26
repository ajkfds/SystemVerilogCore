using System.Collections.Generic;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;
using SystemVerilogCore.Verilog.BuildingBlocks;

namespace SystemVerilogCore.Snippets
{
    /// <summary>
    /// The "Fill Port Connections" tool: rewrite the instantiation at the caret
    /// with one named connection per port, deriving a signal name from each
    /// unconnected port (and declaring it when the enclosing module does not have
    /// it yet).
    /// </summary>
    public static class PortConnectionCreateSnippetBuilder
    {
        /// <summary>Builds the snippet for the caret at <paramref name="index"/>.</summary>
        public static void Build(
            ParsedDocument parsedDocument,
            int index,
            SnippetProvider.SnippetResult result)
        {
            Verilog.Items.ModuleInstantiation? instantiation =
                SnippetBuilder.FindInstantiationAt(parsedDocument, index);
            if (instantiation == null)
            {
                result.Message = "the caret is not inside a module instantiation";
                return;
            }
            if (instantiation.LastIndexReference == null) return;

            Module? module = instantiation.GetInstancedBuildingBlock() as Module;
            if (module == null)
            {
                result.Message = "the instantiated module could not be resolved";
                return;
            }

            int start = SnippetBuilder.LastIndex(instantiation.BeginIndexReference);
            int end = SnippetBuilder.LastIndex(instantiation.LastIndexReference) + 1;
            string indent = SnippetBuilder.GetIndent(parsedDocument, start);

            // The enclosing building block is what decides whether a derived
            // signal name still has to be declared, so the tool has to know it.
            // The replacement range starts inside the instance, so looking the
            // block up at "start" gives the module the instantiation lives in.
            BuildingBlock? enclosing = parsedDocument.GetBuildingBlockAt(start);

            SnippetTemplate template = SnippetBuilder.BuildInstantiationTemplate(
                module, instantiation.Name, indent, instantiation, enclosing);

            List<SnippetField> fields = new();
            string plainText = template.ToPlainText(fields);

            result.Snippets.Add(new SnippetProvider.SnippetEntry
            {
                Label = "portConnectionCreate",
                Detail = "fill the connections of " + instantiation.Name,
                Kind = SnippetProvider.SnippetKind.Replace,
                Start = start,
                End = end,
                SnippetText = template.ToVscodeSnippet(),
                PlainText = plainText,
                Fields = fields,
            });
        }
    }
}
