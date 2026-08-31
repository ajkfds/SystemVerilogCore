using System.Collections.Generic;
using SystemVerilogCore.Documents;

namespace SystemVerilogCore
{
    /// <summary>
    /// Abstraction over a single source file in the SystemVerilog project.
    /// Hosts (the IDE plugin or the LSP) own the actual file on disk and
    /// present this view to the core.
    /// </summary>
    public interface ISystemVerilogFile
    {
        /// <summary>Project-unique identifier (typically a URI or path).</summary>
        string Id { get; }

        /// <summary>Absolute path of the file on disk, if available.</summary>
        string? AbsolutePath { get; }

        /// <summary>True when the file contains SystemVerilog constructs.</summary>
        bool IsSystemVerilog { get; }

        /// <summary>The current code document behind this file.</summary>
        ISystemVerilogCodeDocument CodeDocument { get; }

        /// <summary>
        /// Building blocks declared at the top level of this file. Always
        /// non-null; empty when the file has not been parsed yet.
        /// </summary>
        IReadOnlyList<ISystemVerilogBuildingBlock> TopLevelBlocks { get; }
    }
}
