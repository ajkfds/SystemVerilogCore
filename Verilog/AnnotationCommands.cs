namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// The command words the parser recognises in comment annotations.
    ///
    /// <para>
    /// A source file marks intent with comments such as
    /// <c>// @sync:CLK_I</c> or <c>// @clock</c>. The words themselves are
    /// configurable per project (a team may prefer a different prefix), so the
    /// parser reads them from the project settings rather than hard-coding
    /// them. This type is that table.
    /// </para>
    ///
    /// <para>
    /// It used to be a nested class of the editor's <c>ProjectProperty</c>.
    /// It carries no UI types and the parser is its only consumer, so it moved
    /// here with the rest of the parser. The defaults are unchanged, which is
    /// what keeps existing <c>.project</c> files valid.
    /// </para>
    /// </summary>
    public class AnnotationCommands
    {
        public string Synchronized = "@sync";
        public string SameSync = "@samesync";
        public string Clock = "@clock";
        public string Reset = "@reset";
        public string PortGroup = "@portgroup";
        public string Discard = "@discard";
        public string Unused = "@unused";
        public string RefInstance = "@ref_instance";
        public string Markdown = "@markdown";
        public string ToolOption = "@tool_option";
        public string Scope = "@scope";
    }
}
