using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class ConnCounter
    {
        [PrimaryKey]
        public long ticks { get; set; }

    }
}
