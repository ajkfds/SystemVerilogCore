using System.Collections.Generic;
using SystemVerilogCore.Diagnostics;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// A parse result of a single source file. The implementation may be
    /// incrementally built, but the interface only exposes the result that
    /// consumers (LSP, IDE) need to query.
    /// </summary>
    public interface ISystemVerilogDocument
    {
        ISystemVerilogFile File { get; }

        /// <summary>The root building block (synthetic) that owns all top-level blocks.</summary>
        ISystemVerilogBuildingBlock Root { get; }

        /// <summary>Diagnostics collected while parsing this file.</summary>
        IReadOnlyList<ISystemVerilogDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Returns the deepest named element that contains the given
        /// character index, or <c>null</c> if no element covers the index.
        /// </summary>
        ISystemVerilogNamedElement? FindElementAt(int index);
    }
}
