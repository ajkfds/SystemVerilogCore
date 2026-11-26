using SystemVerilogCore.Verilog.DataObjects.DataTypes;

namespace SystemVerilogCore.Verilog
{
    public interface IPartSelectableDataObject
    {
        public bool PartSelectable { get; }
        public IDataType? ParsePartSelect(WordScanner word, NameSpace nameSpace);
    }
}
