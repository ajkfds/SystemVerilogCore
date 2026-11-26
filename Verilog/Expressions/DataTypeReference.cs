using SystemVerilogCore.Verilog.DataObjects.DataTypes;

namespace SystemVerilogCore.Verilog.Expressions
{
    public class DataTypeReference : Primary
    {
        public IDataType? IDataType { get; set; } = null;
    }
}
