using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmeansTool
{
    public class GuiCommon
    {
        public delegate void Debug(string msg, bool isNewLine = true);
        Debug debug = null;
        public void SetDebugMsgFunction(Debug func)
        {
            this.debug = func;
        }
        public void d(string msg, bool isNewLine = true)
        {
            if (this.debug == null) { return; }
            debug(msg, isNewLine);
        }
    }
}
