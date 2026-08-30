using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemVerilogCore.Documents;

namespace SystemVerilogCore
{
    /// <summary>
    /// A SystemVerilog project. Hosts aggregate files (which may live in
    /// many directories) and a hierarchy of building blocks. The interface is
    /// intentionally minimal: it only exposes what the LSP and other tooling
    /// need to read or trigger a re-parse.
    /// </summary>
    public interface ISystemVerilogProject
    {
        /// <summary>All files currently part of the project.</summary>
        IReadOnlyList<ISystemVerilogFile> Files { get; }

        /// <summary>Returns the file with the given id, or <c>null</c>.</summary>
        ISystemVerilogFile? FindFile(string id);

        /// <summary>
        /// Returns the parse result for the given file. Hosts may parse on
        /// demand and return a freshly produced document; the implementation
        /// is expected to be thread-safe.
        /// </summary>
        Task<ISystemVerilogDocument?> GetDocumentAsync(
            ISystemVerilogFile file,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves the symbol at the given position in the given file.
        /// Returns <c>null</c> if the position is not on a symbol or no
        /// definition is known.
        /// </summary>
        Task<ISystemVerilogNamedElement?> FindDefinitionAsync(
            ISystemVerilogFile file,
            int index,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all references to the symbol that contains the given
        /// index. The first element of the list is the declaration itself
        /// when applicable.
        /// </summary>
        Task<IReadOnlyList<ISystemVerilogNamedElement>> FindReferencesAsync(
            ISystemVerilogFile file,
            int index,
            CancellationToken cancellationToken = default);
    }
}
