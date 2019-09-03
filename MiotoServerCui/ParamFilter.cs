using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    abstract class ParamFilter
    {
        public abstract void update(Param param);
    }
}
