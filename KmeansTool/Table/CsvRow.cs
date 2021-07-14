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
        public int ct { get; set; } = 0;

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
                var gap = Convert.ToInt32(items[2])/2.0;
                var row = new CsvRow()
                {
                    ticks = DateTime.Parse(items[0]).Ticks,
                    mac = Convert.ToInt64(items[1], 16),
                    seq = Convert.ToInt32(items[2]),
                    //btn = Convert.ToInt32(items[3]),
                    //lqi = Convert.ToInt32(items[5]),
                    ct = (int)(Convert.ToDouble(items[6]) * 10.0 / gap)//,
                    //ct01 = (int)(Convert.ToDouble(items[7]) * 10.0 / gap),
                    //ct10 = (int)(Convert.ToDouble(items[8]) * 10.0 / gap),
                    //ct11 = (int)(Convert.ToDouble(items[9]) * 10.0 / gap)
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
