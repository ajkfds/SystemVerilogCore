using System.Collections.Generic;

namespace SystemVerilogCore.Verilog
{
    public interface ICommentAnnotated
    {
        Dictionary<string, string> CommentAnnotations { get; }
        void AppendAnnotation(string key, string value);

    }
}
