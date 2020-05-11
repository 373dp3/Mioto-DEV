using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    class SoundTbl
    {
        //"CREATE TABLE [soundTbl] ([file] TEXT, [ticks] INTEGER, PRIMARY KEY(file)) "
        [PrimaryKey]
        public string file { get; set; }
        public long ticks { get; set; }
    }
}
