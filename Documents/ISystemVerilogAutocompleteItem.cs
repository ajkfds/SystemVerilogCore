using System.Collections.Generic;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// UI-agnostic autocomplete candidate data. Hosts (LSP server, IDE
    /// completion popup, tests) convert this into their own presentation
    /// objects; the core never references UI types.
    /// </summary>
    public interface ISystemVerilogAutocompleteItem
    {
        /// <summary>Text inserted when the candidate is selected.</summary>
        string Label { get; }

        /// <summary>Optional detail line shown next to the label (e.g. type).</summary>
        string? Detail { get; }

        /// <summary>Stable token describing the visual category of the item.</summary>
        SystemVerilogAutocompleteItemKind Kind { get; }
    }

    /// <summary>
    /// Simple immutable implementation of <see cref="ISystemVerilogAutocompleteItem"/>
    /// usable by adapters and tests.
    /// </summary>
    public sealed class SystemVerilogAutocompleteItem : ISystemVerilogAutocompleteItem
    {
        public SystemVerilogAutocompleteItem(string label, SystemVerilogAutocompleteItemKind kind, string? detail = null)
        {
            Label = label;
            Kind = kind;
            Detail = detail;
        }

        public string Label { get; }

        public string? Detail { get; }

        public SystemVerilogAutocompleteItemKind Kind { get; }
    }

    public enum SystemVerilogAutocompleteItemKind
    {
        Unknown = 0,
        Keyword,
        Variable,
        Net,
        Parameter,
        LocalParameter,
        Port,
        Typedef,
        Function,
        Task,
        Module,
        Interface,
        Program,
        Checker,
        Primitive,
        Package,
        Class,
        Instance,
        Macro,
        Snippet,
    }
}
