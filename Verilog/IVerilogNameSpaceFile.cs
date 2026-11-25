namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// The file view a <see cref="RegisterNameSpace{T}"/> needs.
    ///
    /// <para>
    /// A name space maps a declaration name to the file that declared it. To
    /// read the declaration back it needs one thing from that file: the
    /// root-level named element with a given name. Everything else about the
    /// file (its path, its project, whether it is dirty) is irrelevant here.
    /// </para>
    ///
    /// <para>
    /// <b>Why a single member, and no inheritance.</b> The editor's
    /// <c>Data.VerilogFile</c> derives from <c>ITextFile</c>, which already
    /// declares <c>ID</c>, <c>RelativePath</c>, <c>Name</c>, <c>Project</c> and
    /// <c>ParsedDocument</c>. Adding a base interface that declared any of those
    /// again would make every call site through the editor's file type fail with
    /// CS0229 (ambiguous member), and an interface cannot resolve that by
    /// re-declaring them. This contract therefore declares only the member
    /// <c>ITextFile</c> does not have, so the editor keeps its
    /// <c>ITextFile</c> base and simply implements this alongside it.
    /// </para>
    /// </summary>
    public interface IVerilogNameSpaceFile
    {
        /// <summary>
        /// The root-level named element called <paramref name="name"/>, or null
        /// when this file does not declare it.
        ///
        /// <para>
        /// "Root-level" means the file's parse result root: the modules,
        /// interfaces, packages, programs, primitives, checkers and classes
        /// declared outside any other declaration. Implementations read their own
        /// parse result and return the element, so the name space does not need
        /// to know how a host stores it.
        /// </para>
        /// </summary>
        object? FindRootNamedElement(string name);

        /// <summary>Unique id of the file (host-defined).</summary>
        string Id { get; }

        /// <summary>
        /// Name of the project this file belongs to, or an empty string when it
        /// is not attached to one yet.
        ///
        /// <para>
        /// A file reference records this so the host can find the project again
        /// later (<c>// @project &lt;name&gt;</c> resolves by the same key).
        /// </para>
        /// </summary>
        string ProjectName { get; }

        /// <summary>Project-relative path, '/'-separated.</summary>
        string FileRelativePath { get; }
    }
}
