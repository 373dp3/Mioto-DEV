using MiotoServer.Struct;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class ProductionFactor: IParseable
    {
        [PrimaryKey, AutoIncrement]
        public long id { get; set; }

        public enum Validation { VALID=1, INVALID=0}
        public Validation isValid { get; set; } = Validation.VALID;

        public long mac { get; set; } = 0;
        public const long SET_TICKS_AT_SERVER = -1;
        public long stTicks { get; set; } = SET_TICKS_AT_SERVER;

        public const double CT_NOOP = 0;
        public double ct { get; set; } = CT_NOOP;

        public enum Status
        {
            //ライン内
            NOOP=1,
            START_PRODUCTION=200,
            WAITING_FOR_PARTS=210,
            START_REST=220,
            START_PLANNED_STOP=290,

            //ライン外
            START_CHANGE_PRODUCTION=400,
            FINISH_CHANGE_PRODUCTION=490,

            //非常時
            START_BRAKEDOWN_STOP=700,
            START_REPAIRING=800,
            FINISH_REPAIRING=900,
        }
        public Status status { get; set; } = Status.NOOP;

        [StringLength(500, ErrorMessage = "文字数が超過しています500バイトまで)")]
        public string memo { get; set; } = "";

        public string ToCSV()
        {
            return $"{id.ToString("x")}," +
                $"{(int)isValid}," +
                $"{mac.ToString("x")}," +
                $"{stTicks.ToString("x")}," +
                $"{ct.ToString("F1")}," +
                $"{(int)status}," +
                $"{memo}";
        }
        public static ProductionFactor ParseCSV(string csv)
        {
            var item = csv.Split(',');
            var i = 0;
            var ans = new ProductionFactor();
            try
            {
                ans.id = Convert.ToInt64(item[i],16); i++;
                ans.isValid = (Validation)Convert.ToInt64(item[i]); i++;
                ans.mac = Convert.ToInt64(item[i],16); i++;
                ans.stTicks = Convert.ToInt64(item[i],16); i++;
                ans.ct = Convert.ToDouble(item[i]); i++;
                ans.status = (Status)Convert.ToInt64(item[i]); i++;
                ans.memo =item[i]; i++;
            }
            catch (Exception e)
            {
                return null;
            }
            return ans;
        }

        public void ParseInto(string msg)
        {
            var item = msg.Split(',');

            var i = 0;
            id = Convert.ToInt64(item[i], 16); i++;
            isValid = (Validation)Convert.ToInt64(item[i]); i++;
            mac = Convert.ToInt64(item[i], 16); i++;
            stTicks = Convert.ToInt64(item[i], 16); i++;
            ct = Convert.ToDouble(item[i]); i++;
            status = (Status)Convert.ToInt64(item[i]); i++;
            memo = item[i]; i++;
        }

        public const string KEY = "production_factor";
    }
}
