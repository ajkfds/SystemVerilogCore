using SystemVerilogCore.Documents;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Anything that has a name in the SystemVerilog scope tree. The kind
    /// discriminator lets consumers decide how to render the symbol without
    /// having to know the concrete type.
    /// </summary>
    public interface ISystemVerilogNamedElement
    {
        string Name { get; }

        SystemVerilogNamedElementKind Kind { get; }

        /// <summary>
        /// Source range that defines the element. For declarations this is
        /// the identifier token; for references it is the use site. May be
        /// <c>null</c> if the range is not available (e.g. synthesised
        /// root scopes).
        /// </summary>
        SystemVerilogRange? DefinitionRange { get; }

        /// <summary>
        /// Building block that owns this element, or <c>null</c> if it lives
        /// at the top level (e.g. a package declaration in the root file).
        /// </summary>
        ISystemVerilogBuildingBlock? Owner { get; }

        /// <summary>
        /// The file in which this element was declared. Used by consumers to
        /// resolve cross-file references.
        /// </summary>
        ISystemVerilogFile? File { get; }
    }

    public enum SystemVerilogNamedElementKind
    {
        Unknown = 0,
        Module,
        Interface,
        Package,
        Program,
        Checker,
        Primitive,
        Class,
        Variable,
        Net,
        Parameter,
        LocalParameter,
        Port,
        Typedef,
        Function,
        Task,
        Modport,
        Instance,
        PackageItem,
        Macro,
        GenerateBlock,
    }
}
