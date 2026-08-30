namespace SystemVerilogCore.Diagnostics
{
    /// <summary>
    /// Diagnostic severity level for messages produced by the SystemVerilog
    /// language tooling. Mirrors the LSP "DiagnosticSeverity" concept, but is
    /// kept in this assembly so that consumers do not need to take a
    /// dependency on a specific LSP library.
    /// </summary>
    public enum SystemVerilogSeverity
    {
        Hint = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
    }
}
