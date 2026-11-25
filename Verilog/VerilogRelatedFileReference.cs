namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// A reference to another Verilog file, stored by identity rather than by
    /// object.
    ///
    /// <para>
    /// A parse result records the files it depends on (<c>include</c> targets,
    /// instantiated modules) so a later pass can re-parse them. Holding the file
    /// objects would keep them alive, so only the identity is kept: the owning
    /// project's name, the project-relative path and the file id.
    /// </para>
    ///
    /// <para>
    /// This is plain data and the parser is what fills it in, so it lives here.
    /// Resolving the identity back to a live file needs the host's project
    /// registry (the editor's <c>Global.Projects</c>), which the UI-free assembly
    /// cannot reach; that lookup is a host-side extension method over this type
    /// (<c>VerilogRelatedFileReferenceExtensions.GetTargetFile</c> in the editor
    /// plugin).
    /// </para>
    /// </summary>
    public class VerilogRelatedFileReference
    {
        public VerilogRelatedFileReference(IVerilogNameSpaceFile file)
        {
            ID = file.Id;
            ProjectName = file.ProjectName;
            RelativeFilePath = file.FileRelativePath;
        }

        public string ID { set; get; }
        public string ProjectName { get; set; }
        public string RelativeFilePath { get; set; }
    }
}
