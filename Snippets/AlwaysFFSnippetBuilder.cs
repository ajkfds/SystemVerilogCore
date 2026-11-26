using System.Collections.Generic;
using SystemVerilogCore.Verilog;
using SystemVerilogCore.Verilog.BuildingBlocks;

namespace SystemVerilogCore.Snippets
{
    /// <summary>
    /// The "alwaysFF" tool: an <c>always @(posedge clk or negedge rst)</c> block
    /// with the clock / reset taken from the ports the parser marked as clock /
    /// reset (SyncContext).
    ///
    /// <para>
    /// The text is built here and inserted by the host, so the desktop editor and
    /// the VS Code extension insert exactly the same block from one
    /// implementation.
    /// </para>
    ///
    /// <para>
    /// The clock and reset are real tab stops (not baked-in text) so the user can
    /// pick a different one: several annotated clocks / resets become a choice
    /// list (<c>${1|CLK_I,CLK2_I|}</c> in VS Code, the popup the desktop editor
    /// already opened). The <c>if(~reset)</c> is a <b>mirror</b> of the reset tab
    /// stop, so editing one updates the other.
    /// </para>
    ///
    /// <para>
    /// <b>Why this lives here.</b> The snippet used to be a static <c>Build</c>
    /// on the editor's <c>InteractiveSnippet</c> subclass, so the language server
    /// had to reference the editor stack to serve <c>rtl/snippet</c>. The editor
    /// class now delegates its <c>Build</c> here.
    /// </para>
    /// </summary>
    public static class AlwaysFFSnippetBuilder
    {
        /// <summary>Builds the snippet for the caret at <paramref name="index"/>.</summary>
        public static void Build(
            ParsedDocument parsedDocument,
            int index,
            SnippetProvider.SnippetResult result)
        {
            string indent = SnippetBuilder.GetIndent(parsedDocument, index);

            List<string> clocks = new();
            List<string> resets = new();
            BuildingBlock? block = parsedDocument.GetBuildingBlockAt(index);
            if (block is IPortNameSpace portNameSpace)
            {
                foreach (Verilog.DataObjects.Port port in portNameSpace.PortsList)
                {
                    if (port.DataObject == null) continue;
                    if (port.DataObject.SyncContext.IsClock && !clocks.Contains(port.Name)) clocks.Add(port.Name);
                    if (port.DataObject.SyncContext.IsReset && !resets.Contains(port.Name)) resets.Add(port.Name);
                }
            }

            SnippetTemplate template = new SnippetTemplate();
            if (parsedDocument.SystemVerilog)
            {
                template.AppendText(indent + "always_ff @(posedge ");
            }
            else
            {
                template.AppendText(indent + "always @(posedge ");
            }
            template.AddField("clock", clocks.Count > 0 ? clocks[0] : "clock", clocks);
            template.AppendText(" or negedge ");
            int resetField = template.AddField("reset", resets.Count > 0 ? resets[0] : "reset_x", resets);
            template.AppendText(")\n");
            template.AppendText(indent + "begin\n");
            template.AppendText(indent + "\tif(~");
            template.AddMirror(resetField);
            template.AppendText(") begin\n");
            template.AppendText(indent + "\t\t");
            template.AddField("reset-branch", "");
            template.AppendText("\n");
            template.AppendText(indent + "\tend else begin\n");
            template.AppendText(indent + "\t\t");
            template.AddField("else-branch", "");
            template.AppendText("\n");
            template.AppendText(indent + "\tend\n");
            template.AppendText(indent + "end");

            List<SnippetField> fields = new();
            string plainText = template.ToPlainText(fields);

            result.Snippets.Add(new SnippetProvider.SnippetEntry
            {
                Label = "alwaysFF",
                Detail = "always @(posedge " + (clocks.Count > 0 ? clocks[0] : "clock")
                    + " or negedge " + (resets.Count > 0 ? resets[0] : "reset_x") + ")",
                Kind = SnippetProvider.SnippetKind.Insert,
                Start = index,
                End = index,
                SnippetText = template.ToVscodeSnippet(),
                Fields = fields,
                PlainText = plainText,
            });
        }
    }
}
