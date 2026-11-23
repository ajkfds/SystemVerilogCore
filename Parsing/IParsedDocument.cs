using System;

namespace SystemVerilogCore.Parsing
{
    /// <summary>
    /// The minimum a parse result must expose so a host can hold it.
    ///
    /// <para>
    /// The editor used to require its own <c>ParsedDocument</c> base class,
    /// which forced every plugin's parse result to derive from it - and that
    /// inheritance is what tied the SystemVerilog parser to the editor
    /// assembly. This contract carries only what a host needs to cache and
    /// invalidate a result (its key and the document version it came from), so a
    /// parse result can be a plain class.
    /// </para>
    ///
    /// <para>
    /// It lives here rather than in the editor so the parser can implement it
    /// without referencing the editor: <c>CodeEditor2.CodeEditor.IParsedDocument</c>
    /// is now an alias of this type.
    /// </para>
    /// </summary>
    public interface IParsedDocument : IDisposable
    {
        /// <summary>Cache key of this parse result.</summary>
        string Key { get; }

        /// <summary>Document version this result was produced from.</summary>
        ulong Version { get; }
    }
}
