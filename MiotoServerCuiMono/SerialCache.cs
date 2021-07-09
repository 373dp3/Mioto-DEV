using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServerCuiMono
{
    public class SerialCache
    {
        public int mac { get; set; }

        public string key { get; set; }

        public long ticks { get; set; }

        public string csv { get; set; }
    }
}
