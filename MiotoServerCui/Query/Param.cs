using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    public class Param
    {
        public enum VOLUME { NORMAL, THINING, FINAL };
        public enum OPTION { NORMAL, BACKUP };
        public enum TYPE { CT, TWE, PAL, TWE2525 };
        public VOLUME volume { get; internal set; }
        public OPTION option { get; internal set; }
        public TYPE type { get; internal set; }
        public int thiningSec { get; internal set; }
        public List<UInt32> macList { get; internal set; }
        public List<UInt32> dateList { get; internal set; }
        public string url { get; internal set; }
        public const long FIX_ROW_NOOP = 0;
        public long fixRow { get; internal set; }//CSV情報取得時の最大行数

        //macもymdもともに8桁の為、2120年以降をMACと判断。ESPは0x81, TWEは0x7fから始まっているので大丈夫かと
        public long macMin { get; internal set; }

        public Param(string url)
        {
            this.url = url;
            volume = VOLUME.NORMAL;
            option = OPTION.NORMAL;
            type = TYPE.CT;
            macList = new List<uint>();
            dateList = new List<uint>();
            macMin = Convert.ToInt64("21191231", 16);
            fixRow = FIX_ROW_NOOP;
        }
        
        public override string ToString()
        {
            var dayInfo = DateTime.Now.ToString("yyyyMMdd");
            var macInfo = "all";
            if (dateList.Count > 0)
            {
                dayInfo = string.Join("-", dateList);
            }
            if (macList.Count > 0)
            {
                macInfo = "";
                foreach (var mac in macList)
                {
                    if (macInfo.Length == 0)
                    {
                        macInfo = Convert.ToString(mac, 16);
                    }
                    else
                    {
                        macInfo = "_" + Convert.ToString(mac, 16);
                    }
                }
            }
            return type + "_" + dayInfo + "_" + macInfo;
        }
    }
}
