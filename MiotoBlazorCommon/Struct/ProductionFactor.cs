using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiotoBlazorCommon.Struct
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
        [Range(1, double.MaxValue, ErrorMessage = "1以上の値(半角数字)が必要です")]
        public double ct { get; set; } = CT_NOOP;

        public enum Status
        {
            //ライン内
            NOOP=1,                 //無操作で信号が入った場合にもNOOPを活用
            START_PRODUCTION = 200,
            START_PRODUCTION_NOCT = 205,
            WAITING_FOR_PARTS = 210,
            START_REST=220,//復帰は直前の生産要因を参照すること。
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

        [StringLength(500, ErrorMessage = "文字数が超過しています(500バイトまで)")]
        public string memo { get; set; } = "";

        public string ToUserSaveCsv()
        {
            if((this.status != Status.START_PRODUCTION)
                && (this.status != Status.START_PRODUCTION_NOCT))
            {
                return $"\"\"," +
                    new DateTime(stTicks).ToLongTimeString() + "," +
                        (GetDurationSec() / 60).ToString("F1");
            }

            return $"\"{memo}\"," +
                        new DateTime(stTicks).ToLongTimeString() + "," +
                        (GetDurationSec()/60).ToString("F1") + "," +

                        dekidaka + "," +
                        (GetKadouritsu() * 100.0).ToString("F1") + "," +
                        ct.ToString("F1") + "," +
                        aveCt.ToString("F1") + "," +
                        "";
        }
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
        /// <summary>
        /// 要因開始前起動を対象外とした出来高
        /// </summary>
        [Ignore]
        public long dekidaka { get; set; } = 0;
        [Ignore]
        public long lastCycleTicks { get; set; } = 0;
        /// <summary>
        /// 要因開始前起動を対象外とした平均CT
        /// </summary>
        [Ignore]
        public double aveCt { get { return GetAveCt(); } }
        [Ignore]
        public double runSec { get; set; } = 0;
        [Ignore]
        public int signalCount { get; set; } = 0;
        [Ignore]
        public int lostSignalCount { get; set; } = 0;

        [Ignore]
        public bool isFixed { get; set; } = false;

        public double getRunSec(bool isRunning)
        {
            if(isRunning==false) { return runSec; }
            if(isFixed) { return runSec; }

            //以下、稼働中のrunSec扱い

            //より新しい生産要因が存在するか？
            // endTicksがlong.MaxValuなら要因終了未定義、
            // endTicksがlastCycleTicksより大きい場合は、次の要因は無いが、
            // 終了時間が定義された要因
            if (endTicks <= lastCycleTicks)
            {
                // <<履歴の処理中。次に続く要因が確定済み>>
                return runSec;
            }

            // <<現時点でこれより新しい要因は無い>>

            //最終信号から1時間以内の場合は現在時刻を優先し、
            //超過している場合は最終信号を選択する。これは、
            //生産要因を未登録のまま放置した場合の救済用の措置
            if ((lastCycleTicks > 0) && ((new TimeSpan(DateTime.Now.Ticks - lastCycleTicks).TotalHours > 1)))
            {
                return runSec;
            }
            //信号を1度も受信していない状態で、4時間以上経過した場合、
            //0にする。
            if ((lastCycleTicks == 0) && ((new TimeSpan(DateTime.Now.Ticks - stTicks).TotalHours > 4)))
            {
                return 0;
            }

            //lastCycleTicksからの現在の経過時間を加味する
            var progSec = new TimeSpan(DateTime.Now.Ticks - lastCycleTicks).TotalSeconds;
            return runSec + progSec;
        }

        public double getStopSec(bool isRunning)
        {
            return new TimeSpan(GetDurationTicks()).TotalSeconds - getRunSec(isRunning);
        }

        public string GetDuration()
        {
            return getSecString(new TimeSpan(GetDurationTicks()));
        }
        public string getSecString(TimeSpan ts)
        {
            if (ts.TotalSeconds < 60)
            {
                return ts.TotalSeconds.ToString("F1")+"秒";
            }
            if (ts.TotalSeconds < 3600)
            {
                return (ts.TotalMinutes).ToString("F1") + "分";
            }
            return (ts.TotalHours).ToString("F1") + "時間";
        }

        public bool isInnerTimeRange(CycleTime cycle, bool isCheckSignalOrigin = false)
        {
            //稼働開始前なら無視
            if (cycle.dt.Ticks < stTicks) { return false; }
            //稼働終了後も無視
            if (cycle.dt.Ticks > endTicks) { return false; }

            if (isCheckSignalOrigin)
            {
                //停止時間または稼働時間の分だけ始点を戻して評価する
                if (cycle.dt.AddSeconds(-1 * (cycle.ct01 + cycle.ct10)).Ticks < stTicks)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Cycle情報で内部情報を更新する
        /// </summary>
        /// <param name="cycle"></param>
        /// <returns>時刻範囲外:false, 範囲内:true</returns>
        public bool updateByCycle(CycleTime cycle)
        {
            //範囲外なら処理しない
            if (isInnerTimeRange(cycle) == false) { return false; }

            //生産中以外の場合は処理しない
            if ((status != Status.START_PRODUCTION)
                &&(status != Status.START_PRODUCTION_NOCT)) { return true; }

            //信号状況の把握
            signalCount++;
            lostSignalCount += Math.Max(cycle.seq - 1, 0);
            lastCycleTicks = cycle.dt.Ticks;

            //MT状況把握 信号の原点時刻も要因の時刻範囲内であることを保証する
            if (cycle.ct10 > 0)
            {
                var orgDt = cycle.dt.AddSeconds(-1 * cycle.ct10);
                if(orgDt.Ticks < stTicks)
                {
                    //信号原点から要因開始時刻までの超過分を控除
                    runSec += (orgDt - new DateTime(stTicks)).TotalSeconds;
                }
                runSec += cycle.ct10;
            }

            //マシン稼働が終了したときが積算対象
            if (cycle.ct00 == 0) { return true; }


            //要因開始時間前に起動した処理は無効とします。
            //これは前日の扉が閉まる信号を引き継いでしまう問題に
            //対処することが目的です。
            var startDt = cycle.dt.AddSeconds(-1 * cycle.ct10);
            if (startDt.Ticks <  this.stTicks) { return true; }

            dekidaka++;

            return true;
        }
        public double GetDurationSec()
        {
            return new TimeSpan(GetDurationTicks()).TotalSeconds;
        }
        public long GetDurationTicks()
        {
            long bunbo;
            if (lastCycleTicks == 0)
            {
                //1日の最後に指定して終わりが未定義のものを積算するわけには行かないので0
                if (endTicks == long.MaxValue) { return 0; }
            }

            //より新しい生産要因が存在するか？
            if (!isFixed)
            {
                //最終信号から1時間以内の場合は現在時刻を優先し、
                //超過している場合は最終信号を選択する。これは、
                //生産要因を未登録のまま放置した場合の救済用の措置
                if ((lastCycleTicks > 0) && ((new TimeSpan(DateTime.Now.Ticks - lastCycleTicks).TotalHours > 1)))
                {
                    return lastCycleTicks - stTicks;
                }
                //信号を1度も受信していない状態で、4時間以上経過した場合、
                //0にする。
                if ((lastCycleTicks == 0) && ((new TimeSpan(DateTime.Now.Ticks - stTicks).TotalHours > 4)))
                {
                    return 0;
                }
                return DateTime.Now.Ticks - stTicks;
            }
            else
            {
                //次の要因が登録された場合、その設定時刻が
                //計算対象の母数となる
                bunbo = endTicks - stTicks;
            }
            return bunbo;
        }
        private double GetAveCt()
        {
            if(dekidaka==0) { return 0; }
            return (new TimeSpan(GetDurationTicks()).TotalSeconds) / dekidaka;
        }

        /// <summary>
        /// 可動率(%ではなく0～1の比率)
        /// </summary>
        /// <returns></returns>
        public double GetKadouritsu()
        {
            if(dekidaka==0) { return 0; }
            long bunbo = GetDurationTicks();
            if (bunbo<=0) { return 0; }

            var span = new TimeSpan(bunbo);

            //NOCT対応
            if (ct == 0)
            {
                return runSec / span.TotalSeconds;
            }

            return (ct * dekidaka) / span.TotalSeconds;

        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static string GetStatusStr(Status status, bool isTrimStart=false)
        {
            var name = "";
            switch (status)
            {
                case ProductionFactor.Status.NOOP:
                    break;
                case ProductionFactor.Status.START_PRODUCTION:
                    name = "生産開始";
                    break;
                case ProductionFactor.Status.START_PRODUCTION_NOCT:
                    name = "生産(CT無指定)";
                    break;
                case ProductionFactor.Status.WAITING_FOR_PARTS:
                    name = "手待ち";
                    break;
                case ProductionFactor.Status.START_REST:
                    name = "休憩開始";
                    break;
                case ProductionFactor.Status.START_PLANNED_STOP:
                    name = "計画停止";
                    break;

                //ライン外
                case ProductionFactor.Status.START_CHANGE_PRODUCTION:
                    name = "段替え開始";
                    break;
                case ProductionFactor.Status.FINISH_CHANGE_PRODUCTION:
                    name = "段替え終了";
                    break;

                //非常時
                case ProductionFactor.Status.START_BRAKEDOWN_STOP:
                    name = "設備異常";
                    break;
                case ProductionFactor.Status.START_REPAIRING:
                    name = "修理開始";
                    break;
                case ProductionFactor.Status.FINISH_REPAIRING:
                    name = "修理終了";
                    break;
            }
            if (isTrimStart)
            {
                return name.Replace("開始", "");
            }
            return name;
        }
        #endregion
    }
}
