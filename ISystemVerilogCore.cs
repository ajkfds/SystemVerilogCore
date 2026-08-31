using System.Threading;
using System.Threading.Tasks;
using SystemVerilogCore.Documents;

namespace SystemVerilogCore
{
    /// <summary>
    /// Top-level entry point for the SystemVerilog language tooling. The host
    /// (IDE or LSP) creates an implementation and uses it to obtain a
    /// project handle, which in turn exposes the actual documents.
    /// </summary>
    public interface ISystemVerilogCore
    {
        /// <summary>Returns the project with the given id, creating it on demand.</summary>
        Task<ISystemVerilogProject> GetProjectAsync(
            string projectId,
            CancellationToken cancellationToken = default);
    }
}
