using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    public class Param
    {
        public enum VOLUME { NORMAL, THINING, FINAL };
        public enum OPTION { NORMAL, BACKUP };
        public enum TYPE { CT, TWE, PAL, TWE2525, AH, 
            MEM_DB=101,
            PRODUCTION_FACTOR = 201,
        };
        public VOLUME volume { get; set; }
        public OPTION option { get; set; }
        public TYPE type { get; internal set; }
        public int thiningSec { get; internal set; }
        public List<UInt32> macList { get; set; }
        public List<UInt32> dateList { get; set; }
        public string url { get; internal set; }
        public const long FIX_ROW_NOOP = 0;
        public long fixRow { get; internal set; }//CSV情報取得時の最大行数

        public string memDbKey { get; set; } = "";

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
        public List<DateTime> GetDateTimes()
        {
            return dateList.Select(q => GetDateTimeByYYYYMMDD(q.ToString("D8"))).ToList();
        }
        public DateTime GetToday_HHMM()
        {
            var today = DateTime.Now;
            var tmpDt = GetDateTimeByYYYYMMDD(today.ToString("yyyyMMdd"));
            //24:00をまわっていると、現在時刻よりも先の時刻が返されるため、戻す
            if (tmpDt > today)
            {
                return tmpDt.AddDays(-1);
            }
            return tmpDt;
        }
        private DateTime GetDateTimeByYYYYMMDD(string date)
        {
            return DateTime.ParseExact(date + $" {MiotoServerWrapper.config.hhmm.ToString("D4")}", 
                $"yyyyMMdd HHmm", 
                CultureInfo.InvariantCulture);
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
