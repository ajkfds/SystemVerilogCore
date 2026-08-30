using SystemVerilogCore.Documents;

namespace SystemVerilogCore.Diagnostics
{
    /// <summary>
    /// A single diagnostic message produced during parsing, type checking or
    /// rule evaluation. Independent of the host UI; consumers (the IDE plugin
    /// or the LSP) decide how to surface them.
    /// </summary>
    public interface ISystemVerilogDiagnostic
    {
        SystemVerilogSeverity Severity { get; }
        string Code { get; }
        string Message { get; }
        SystemVerilogRange Range { get; }
    }
}
