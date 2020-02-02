using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServerCuiMono.mioto_db
{
    public class latestinfo
    {
        //2019段階のsqlite-netは、複合主キーを扱うことが出来ない
        //CREATE TABLE [latestinfo] ([mac] INTEGER, [seq] INTEGER, [btn] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, PRIMARY KEY(mac, btn));
        //[PrimaryKey]
        public long mac { get; set; }
        public int seq { get; set; }
        //[PrimaryKey]
        public int btn { get; set; }
        public int batt { get; set; }
        public long ticks { get; set; }
    }
}
