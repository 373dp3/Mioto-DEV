using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class LastInfo
    {
        //"CREATE TABLE [latestinfo] ([mac] INTEGER, [seq] INTEGER, [btn] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, PRIMARY KEY(mac, btn));",
        //から複合主キーを排除する。ORMで削除しやすいようにidを含める。
        [PrimaryKey, AutoIncrement]
        public long id { get; set; }
        public long mac { get; set; }
        public int seq { get; set; }
        public int btn { get; set; }
        public int lqi { get; set; }
        public int batt { get; set; }
        public long ticks { get; set; }
    }
}
