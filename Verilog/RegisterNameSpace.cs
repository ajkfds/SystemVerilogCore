using System;
using System.Collections.Generic;
using System.Threading;

namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// A project-wide name space that maps a declaration name to the file that
    /// declared it.
    ///
    /// <para>
    /// The parser registers top-level declarations here as it parses
    /// (<c>DefinitionNameSpace</c> for modules / interfaces / programs /
    /// primitives, <c>PackageNameSpace</c> for packages, <c>UnitNameSpace</c>
    /// for root-level classes and other compilation-unit-scope names). A later
    /// parse then resolves a name by asking the same table for the declaring
    /// file and reading that file's parse result.
    /// </para>
    ///
    /// <para>
    /// The table holds files weakly: a file that has been closed and collected
    /// simply disappears, so lookups read as "not registered" rather than
    /// keeping every parsed file alive for as long as the project is open.
    /// </para>
    ///
    /// <para>
    /// <b>How the file is used.</b> Only two things are needed from it: its parse
    /// result (to read the declared element back) and its identity (so a host
    /// can reach the same file again). Both are expressed as the UI-free
    /// <see cref="IVerilogNameSpaceFile"/> contract, which is what lets this
    /// type live in the UI-free assembly. The editor's
    /// <c>Data.VerilogFile</c> implements that contract by delegating to its own
    /// parse result, so the editor keeps its <c>ITextFile</c> base.
    /// </para>
    /// </summary>
    public class RegisterNameSpace<T> where T : class
    {
        // Thread-safe access using ReaderWriterLockSlim
        private readonly ReaderWriterLockSlim accessLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        // Declaration name -> declaring file (held weakly).
        private readonly WeakReferenceDictionary<string, IVerilogNameSpaceFile> fileTable =
            new WeakReferenceDictionary<string, IVerilogNameSpaceFile>();

        /// <summary>
        /// Records that <paramref name="buildingBlockName"/> is declared in
        /// <paramref name="file"/>.
        /// </summary>
        public void Register(string buildingBlockName, T buildingBlock, IVerilogNameSpaceFile? file)
        {
            if (file == null)
            {
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                return;
            }

            accessLock.EnterWriteLock();
            try
            {
                fileTable.Register(buildingBlockName, file);
            }
            finally
            {
                accessLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// The file that declared <paramref name="name"/>, or null when the name
        /// is unknown or the file has been collected.
        /// </summary>
        public IVerilogNameSpaceFile? GetFile(string name)
        {
            accessLock.EnterReadLock();
            try
            {
                return _GetFile(name);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        private IVerilogNameSpaceFile? _GetFile(string name)
        {
            return fileTable.GetItem(name);
        }

        /// <summary>
        /// Every registered name whose declaration matches
        /// <paramref name="isMatched"/>.
        /// </summary>
        public List<string> GetNameList(Func<T, bool> isMatched)
        {
            List<string> results = new List<string>();

            accessLock.EnterReadLock();
            try
            {
                foreach (var key in fileTable.KeyList())
                {
                    T? target = _Get(key);
                    if (target == null) continue;
                    if (isMatched(target)) results.Add(key);
                }
                return results;
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Resolves <paramref name="name"/> to its declared element, or null.
        /// </summary>
        public T? Get(string name)
        {
            accessLock.EnterReadLock();
            try
            {
                return _Get(name);
            }
            finally
            {
                accessLock.ExitReadLock();
            }
        }

        private T? _Get(string name)
        {
            IVerilogNameSpaceFile? file = _GetFile(name);
            if (file == null) return null;

            return file.FindRootNamedElement(name) as T;
        }
    }
}
