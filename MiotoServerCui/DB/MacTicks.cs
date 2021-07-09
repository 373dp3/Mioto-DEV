using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    class MacTicks
    {
        [PrimaryKey]
        public long mac { get; set; }

        public long ticks { get; set; }
    }
}
