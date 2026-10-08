using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A named declaration that can contain other symbols in SystemVerilog
    /// (module, interface, package, checker, program, primitive, class, ...).
    /// </summary>
    public interface ISystemVerilogBuildingBlock : ISystemVerilogNamedElement
    {
        /// <summary>Kind of the building block (module, interface, ...).</summary>
        new SystemVerilogBuildingBlockKind Kind { get; }

        /// <summary>
        /// Children of this block, keyed by their declared name. Sub-blocks
        /// such as generate scopes or nested modules are reachable here.
        /// </summary>
        IReadOnlyDictionary<string, ISystemVerilogBuildingBlock> BuildingBlocks { get; }

       /// <summary>
       /// All named elements (variables, ports, parameters, ...) directly
       /// declared inside this block.
       /// </summary>
       IReadOnlyList<ISystemVerilogNamedElement> Members { get; }

       /// <summary>
       /// UI-agnostic autocomplete candidates for symbols declared in this
       /// block. Hosts convert them into their own presentation objects.
       /// </summary>
       IReadOnlyList<ISystemVerilogAutocompleteItem> AutocompleteItems { get; }
    }

    public enum SystemVerilogBuildingBlockKind
    {
        Unknown = 0,
        Module,
        Interface,
        Package,
        Program,
        Checker,
        Primitive,
        Class,
        GenerateBlock,
        Root,
    }
}
