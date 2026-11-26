using System.Collections.Generic;
using System.Linq;
using SystemVerilogCore.Documents;
using SystemVerilogCore.Verilog;
using SystemVerilogCore.Verilog.BuildingBlocks;

namespace SystemVerilogCore.Snippets
{
    /// <summary>
    /// The "modport generate" tool: a modport listing every logic signal of the
    /// enclosing interface, with the direction flipped for signals an existing
    /// modport already drives.
    /// </summary>
    public static class ModPortSnippetBuilder
    {
        /// <summary>Builds the snippet for the caret at <paramref name="index"/>.</summary>
        public static void Build(
            ParsedDocument parsedDocument,
            int index,
            SnippetProvider.SnippetResult result)
        {
            BuildingBlock? block = parsedDocument.GetBuildingBlockAt(index);
            if (block is not Interface interface_)
            {
                result.Message = "the caret is not inside an interface";
                return;
            }

            // logic signals default to output; a signal an existing modport
            // already drives becomes an input (the same rule as the editor).
            Dictionary<string, ModPort.Port.DirectionEnum> directions = new();
            foreach (INamedElement element in interface_.NamedElements.Values)
            {
                if (element is Verilog.DataObjects.Variables.Logic)
                {
                    directions[element.Name] = ModPort.Port.DirectionEnum.Output;
                }
            }
            if (directions.Count == 0)
            {
                result.Message = "the interface has no logic signal to expose";
                return;
            }

            foreach (ModPort modport in interface_.NamedElements.Values.OfType<ModPort>())
            {
                foreach (ModPort.Port port in modport.Ports.Values)
                {
                    string expName = port.Expression.CreateString();
                    if (!directions.ContainsKey(expName)) continue;
                    if (port.Direction == ModPort.Port.DirectionEnum.Output
                        || port.Direction == ModPort.Port.DirectionEnum.Inout)
                    {
                        directions[expName] = ModPort.Port.DirectionEnum.Input;
                    }
                }
            }

            string indent = SnippetBuilder.GetIndent(parsedDocument, index);
            SnippetTemplate template = new SnippetTemplate();
            template.AppendText(indent + "modport ");
            template.AddField("modport-name", "modportname");
            template.AppendText(" (\n");
            bool first = true;
            foreach (KeyValuePair<string, ModPort.Port.DirectionEnum> entry in directions)
            {
                if (!first) template.AppendText(",\n");
                template.AppendText(indent + "\t");
                switch (entry.Value)
                {
                    case ModPort.Port.DirectionEnum.Input:
                        template.AppendText("input");
                        break;
                    case ModPort.Port.DirectionEnum.Inout:
                        template.AppendText("inout");
                        break;
                    default:
                        template.AppendText("output");
                        break;
                }
                template.AppendText("\t." + entry.Key);
                first = false;
            }
            if (!first) template.AppendText("\n");
            template.AppendText(indent + ")");

            List<SnippetField> fields = new();
            string plainText = template.ToPlainText(fields);

            result.Snippets.Add(new SnippetProvider.SnippetEntry
            {
                Label = "modport generate",
                Detail = directions.Count + " signal(s)",
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
