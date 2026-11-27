using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// What a node of the hierarchy tree represents.
    ///
    /// <para>
    /// The language server builds the "RTL Hierarchy" tree from the host's item
    /// tree, so every node it sees is a host type
    /// (<c>VerilogModuleInstance</c>, <c>InterfaceInstance</c>,
    /// <c>ImportedPackage</c>, <c>VerilogHeaderInstance</c>). Deciding a node's
    /// icon, its detail text and whether it can be expanded only needs the kind,
    /// so the kind is what this contract exposes instead of the four type names.
    /// </para>
    /// </summary>
    public enum VerilogHierarchyNodeKind
    {
        /// <summary>Not a hierarchy node (a plain file, a folder, ...).</summary>
        None,

        /// <summary>An instance of a module.</summary>
        ModuleInstance,

        /// <summary>An instance of an interface.</summary>
        InterfaceInstance,

        /// <summary>A package imported into a file / instance.</summary>
        ImportedPackage,

        /// <summary>An <c>`include</c>d header, as seen from the including file.</summary>
        IncludeHeader,
    }

    /// <summary>
    /// The extra capabilities the hierarchy view needs from a node of the host's
    /// item tree.
    ///
    /// <para>
    /// The language server used to <c>switch</c> on the editor's concrete node
    /// types to answer three questions: what kind of node is this, what is its
    /// readable name, and (for an instance) which instantiation created it. That
    /// is what kept the hierarchy listing bound to the editor assembly. Each of
    /// those answers is a plain value, so they are on a contract and the host
    /// implementations forward to the members they already have.
    /// </para>
    ///
    /// <para>
    /// <b>Why the member names differ.</b> The editor's node base
    /// (<c>InstanceTextFile</c>) already declares <c>InstanceName</c> and
    /// <c>ParameterOverrides</c>. Declaring either again on a second interface
    /// makes every call site through the editor's types fail with CS0229
    /// (ambiguous member), which is the trap recorded for
    /// <see cref="IVerilogSourceFile"/>. The contract therefore uses distinct
    /// names for the same values.
    /// </para>
    /// </summary>
    public interface IVerilogHierarchyNode
    {
        /// <summary>
        /// What this node represents. Drives the tree icon and the
        /// expandability fallback, so it must be stable for the node's lifetime.
        /// </summary>
        VerilogHierarchyNodeKind HierarchyNodeKind { get; }

        /// <summary>
        /// The readable name shown next to the node: the instance name for a
        /// module / interface instance, the package name for an imported
        /// package, the path for an include header.
        /// </summary>
        string HierarchyNodeName { get; }

        /// <summary>
        /// The instantiation this node was created from, or null when the node
        /// has none (a package, a header, or an instance built without one).
        ///
        /// <para>
        /// Needed for two things: the click position (the instantiation's
        /// reference is where the instance is written) and the parameter
        /// overrides. Kept as the parser's own interface so no editor type is
        /// named.
        /// </para>
        /// </summary>
        Verilog.Items.IBuildingBlockInstantiation? InstantiatingItem { get; }

        /// <summary>
        /// The parameter overrides written at the instantiation, so the click
        /// path can parse the module again with them applied.
        ///
        /// <para>
        /// Empty for a node that has none. Same value as the editor's
        /// <c>InstanceTextFile.ParameterOverrides</c>.
        /// </para>
        /// </summary>
        Dictionary<string, Verilog.Expressions.Expression> HierarchyParameterOverrides { get; }
    }
}
