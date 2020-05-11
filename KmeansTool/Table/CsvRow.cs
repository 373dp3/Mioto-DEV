using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmeansTool.Table
{
    class CsvRow
    {
        public long ticks { get; set; } = 0;
        public long mac { get; set; } = 0;
        public int seq { get; set; } = 0;
        public int btn { get; set; } = 0;
        public int lqi { get; set; } = 0;
        public int ct00 { get; set; } = 0;
        public int ct01 { get; set; } = 0;
        public int ct10 { get; set; } = 0;
        public int ct11 { get; set; } = 0;

        public static CsvRow FromCsv(string csv)
        {
            string[] items = csv.Split(',');
            if(items.Length < 9) { return null; }
            for (var i = 0; i < items.Length; i++)
            {
                if(items[i].Length==0) { items[i] = "0"; }
            }
            try
            {
                var row = new CsvRow()
                {
                    ticks = DateTime.Parse(items[0]).Ticks,
                    mac = Convert.ToInt64(items[1], 16),
                    seq = Convert.ToInt32(items[2]),
                    btn = Convert.ToInt32(items[3]),
                    lqi = Convert.ToInt32(items[5]),
                    ct00 = (int)(Convert.ToDouble(items[6]) * 10.0),
                    ct01 = (int)(Convert.ToDouble(items[7]) * 10.0),
                    ct10 = (int)(Convert.ToDouble(items[8]) * 10.0),
                    ct11 = (int)(Convert.ToDouble(items[9]) * 10.0)
                };
                return row;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
