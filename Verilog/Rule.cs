namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// One parser rule: a stable name plus the severity its diagnostic is
    /// reported with.
    ///
    /// <para>
    /// The name / message / description are fixed by the parser (they describe
    /// what the rule detects); only <see cref="Severity"/> is configurable, per
    /// directory, through a <c>.rule.yaml</c> file (see <see cref="RuleSet"/>).
    /// </para>
    ///
    /// <para>
    /// This used to live in the editor plugin assembly. It carries no UI types,
    /// and the parser is its only consumer, so it belongs with the parser.
    /// </para>
    /// </summary>
    public class Rule
    {
        public enum SeverityEnum
        {
            Error,
            Warning,
            Notice
        }
        public required string Name { get; init; }
        public required SeverityEnum Severity { get; init; }
        public required string Message { get; init; }
        public required string Description { get; init; }
    }
}
