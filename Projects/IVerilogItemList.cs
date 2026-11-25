namespace SystemVerilogCore.Projects
{
    /// <summary>
    /// The mutable child collection of a tree item, as the parser needs it.
    ///
    /// <para>
    /// The parser normally only reads the tree (<see cref="IVerilogItemTree"/>).
    /// One path writes to it: when a module instance is parsed on demand with
    /// parameter overrides, the resulting instance has to be recorded under its
    /// parent so the editor's tree shows it. That write went straight to the
    /// editor's <c>Item.ItemList</c>, which is what kept the parse site bound to
    /// the editor assembly.
    /// </para>
    ///
    /// <para>
    /// This contract is the write surface only: add or replace a child by name.
    /// The editor's <c>Item.ItemList</c> implements it; the language server's file
    /// has no item tree and is not asked to.
    /// </para>
    /// </summary>
    public interface IVerilogItemList
    {
        /// <summary>
        /// Adds <paramref name="item"/> under <paramref name="key"/>, replacing
        /// any child already registered under that key.
        /// </summary>
        void AddOrUpdate(string key, IVerilogItem item);
    }
}
