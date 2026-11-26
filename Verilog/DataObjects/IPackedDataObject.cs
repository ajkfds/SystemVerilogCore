using System.Collections.Generic;

namespace SystemVerilogCore.Verilog.DataObjects
{
    internal interface IPackedDataObject
    {
        List<DataObjects.Arrays.PackedArray> PackedDimensions { get; }
    }
}
