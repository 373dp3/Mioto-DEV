using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    class ConfigTbl
    {
        [PrimaryKey]
        public string key { get; set; }
        public string json { get; set; }

    }
}
