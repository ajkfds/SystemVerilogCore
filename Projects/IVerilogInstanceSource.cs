namespace SystemVerilogCore.Projects
{
    /// <summary>
    /// A module instance, as the parser sees it when it has to parse one on
    /// demand.
    ///
    /// <para>
    /// <b>Why this exists.</b> A module instantiated with parameter overrides
    /// (<c>child #(.WIDTH(4)) u(...)</c>) has to be parsed again with those
    /// overrides applied before the port widths and <c>generate</c> branches can
    /// be checked. The parser does that itself, but it needs two things from the
    /// host to drive it: a parser over the instance (so the overrides are
    /// applied) and a place in the item tree to record the result.
    /// </para>
    ///
    /// <para>
    /// The editor's instance is <c>VerilogModuleInstance</c>, which is an
    /// <c>ITextFile</c>-derived item-tree node - a host type the parser cannot
    /// name. So the host makes one and hands it over as this contract. The
    /// language server has no item tree and returns null, in which case the
    /// parser still resolves the instance but records nothing.
    /// </para>
    ///
    /// <para>
    /// <b>Why the two base interfaces do not collide.</b>
    /// <see cref="Documents.IVerilogSourceFile"/> and <see cref="IVerilogItem"/>
    /// declare no member in common (<c>Id</c> / <c>VerilogProject</c> /
    /// <c>ChildItems</c> versus <c>Name</c> / <c>RelativePath</c> /
    /// <c>Items</c>), so combining them here cannot produce the CS0229 ambiguity
    /// that a second declaration of <c>Name</c> or <c>RelativePath</c> would.
    /// </para>
    /// </summary>
    public interface IVerilogInstanceSource : Documents.IVerilogSourceFile, IVerilogItem
    {
    }
}
