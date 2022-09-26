using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweLitePalToSQLite
{
    public class TableTweLitePal
    {
        [SQLite.PrimaryKey]
        public string datetime { get; set; } = "";
        public long mac { get; set; } = 0;
        public float temperature { get; set; } = 30.0f;
        public float humidity { get; set; } = 50.0f;
        public float battery { get; set; } = 2.1f;
        public int lqi { get; set; } = 0;
        public int seq { get; set; } = 0;
    }
}
