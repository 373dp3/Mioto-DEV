using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.Struct
{
    public interface IParseable
    {
        void ParseInto(string msg);
    }
}
