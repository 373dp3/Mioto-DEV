using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class csvcash
    {
        //                "CREATE TABLE [csvcash] ([mac] INTEGER, [ticks] INTEGER, [csv] TEXT, [flg] INTEGER);",
        public long mac { get; set; }
        public long ticks { get; set; }
        public string csv { get; set; }
        public int flg { get; set; }
    }
}
