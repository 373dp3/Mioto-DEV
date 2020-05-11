using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class latest2525
    {
        //                "CREATE TABLE [latest2525] ([mac] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, [state] INTEGER, PRIMARY KEY(mac));",
        [PrimaryKey]
        public long mac { get; set; }
        public int lqi { get; set; }
        public int batt { get; set; }
        public long ticks { get; set; }
        public int state { get; set; }
    }
}
