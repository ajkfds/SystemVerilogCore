using SystemVerilogCore.Documents;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Optional extension point that backs
    /// <see cref="HoverContent.Build(ISystemVerilogNamedElement)"/>. The
    /// default implementation produces a short kind/name summary that is
    /// safe to use from any host. Hosts that have richer information
    /// (for example, a plugin that knows the element's data type and
    /// bit width) can register a custom provider to add it.
    /// </summary>
    public interface IHoverContentProvider
    {
        /// <summary>
        /// Returns a richer description for the given element, or
        /// <c>null</c> to fall back to the default.
        /// </summary>
        string? Build(ISystemVerilogNamedElement element);
    }
}
