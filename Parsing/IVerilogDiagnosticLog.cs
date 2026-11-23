namespace SystemVerilogCore.Parsing
{
    /// <summary>
    /// A sink for the parser's free-form progress / diagnostic messages.
    ///
    /// <para>
    /// The parser reports problems through <c>WordScanner.AddError</c> &amp;
    /// friends, which are attached to the document and surfaced as diagnostics.
    /// A few sites also write a plain log line (for example "## unfound module
    /// X at Y") purely to help a human follow what the parse did. Those used to
    /// call the editor's static <c>Controller.AppendLog</c> directly, which
    /// tied the parser to the editor assembly and left nothing to call in a
    /// headless host.
    /// </para>
    ///
    /// <para>
    /// The host supplies the sink through
    /// <see cref="Documents.IVerilogTextDocument"/>'s owner - in practice the
    /// project settings object - so the parser only sees this interface. Hosts
    /// without a log view (the language server) return null and the message is
    /// simply dropped; a log line is never load-bearing.
    /// </para>
    /// </summary>
    public interface IVerilogDiagnosticLog
    {
        /// <summary>
        /// Writes one informational line. Implementations must not throw and
        /// must be safe to call from a background parse thread.
        /// </summary>
        void WriteLine(string message);

        /// <summary>
        /// Writes one line the parser considers noteworthy (a missed
        /// resolution it recovered from, ...).
        /// </summary>
        void WriteWarning(string message);
    }
}
