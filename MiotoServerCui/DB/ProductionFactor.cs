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
        /// <summary>
        /// 標準サイクルタイム(秒)
        /// </summary>
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

        #region DBに登録しない生産分析用の変数・メソッド群
        [Ignore]
        public long endTicks { get; set; } = long.MaxValue;
        [Ignore]
        public double totalCt00 { get; set; } = 0;
        [Ignore]
        public long dekidaka { get; set; } = 0;
        [Ignore]
        public long lastCycleTicks { get; set; } = 0;
        [Ignore]
        public double aveCt { get; set; } = 0;
 
        public void updateByCycle(CycleTime cycle)
        {
            //生産中以外の場合は処理しない
            if(status != Status.START_PRODUCTION) { return; }
            //マシン稼働が終了したときが積算対象
            if (cycle.ct00 == 0) { return; }
            //稼働開始前なら無視
            if(cycle.dt.Ticks<stTicks) { return; }
            //稼働終了後も無視
            if(cycle.dt.Ticks>endTicks) { return; }

            totalCt00 += cycle.ct00;
            dekidaka++;
            aveCt = (aveCt * (dekidaka - 1) + cycle.ct00) / dekidaka;
            lastCycleTicks = cycle.dt.Ticks;
        }

        /// <summary>
        /// 可動率(%ではなく0～1の比率)
        /// </summary>
        /// <returns></returns>
        public double GetKadouritsu()
        {
            if(dekidaka==0) { return 0; }
            long bunbo;
            if (endTicks == long.MaxValue)
            {
                //次の余韻が登録されていない場合、最終信号時刻が
                //計算対象の母数となる。
                bunbo = lastCycleTicks - stTicks;
            }
            else
            {
                //次の要因が登録された場合、その設定時刻が
                //計算対象の母数となる
                bunbo = endTicks - stTicks;
            }
            if(bunbo<=0) { return 0; }

            var span = new TimeSpan(bunbo);

            return (ct * dekidaka) / span.TotalSeconds;

        }
        #endregion
    }
}
