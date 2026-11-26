using System.Collections.Generic;

namespace SystemVerilogCore.Verilog
{
    public class Comment
    {

        public void ParseComment(string fullComment, out string followedComment, out List<string> tags)
        {
            tags = null;
            followedComment = fullComment;
        }
    }
}
