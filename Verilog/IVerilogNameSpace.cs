using System;
using System.Collections.Generic;

namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// The non-generic view of a <see cref="RegisterNameSpace{T}"/>.
    ///
    /// <para>
    /// The parser's project settings expose three name spaces (definitions,
    /// packages, compilation units). Each is a
    /// <see cref="RegisterNameSpace{T}"/> over a different element type, so a
    /// settings contract cannot name one concrete generic instantiation. This
    /// interface is what the settings contract carries; the parser narrows the
    /// result to the element type it expects at the call site.
    /// </para>
    /// </summary>
    public interface IVerilogNameSpace
    {
        /// <summary>
        /// The declared element called <paramref name="name"/>, or null.
        /// </summary>
        object? GetElement(string name);

        /// <summary>
        /// The file that declared <paramref name="name"/>, or null.
        /// </summary>
        IVerilogNameSpaceFile? GetDeclaringFile(string name);

        /// <summary>
        /// Every registered name whose declaration matches
        /// <paramref name="isMatched"/>.
        /// </summary>
        List<string> GetNameList(Func<object, bool> isMatched);
    }
}
