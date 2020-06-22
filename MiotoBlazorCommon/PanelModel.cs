using MiotoBlazorCommon;
using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MiotoBlazorCommon
{
    public class PanelModel
    {     
        public string title { get; set; }
        public long mac { get; set; } = 0;

        public RunOrStop status { get; private set; } = RunOrStop.NOOP;
        public long signalNum { get; set; } = 0;
        public CycleTime lastCycleTime { get; private set; } = new CycleTime();

        private ProductionFactorHelper productionHelper = new ProductionFactorHelper();

        public static long CreateEndTicks(DateTime dt)
        {
            return DateTime.Parse((dt.AddHours(ProductionFactorHelper.SummaryDurationHour)).ToString("yyyy/MM/dd HH") + ":00:00").Ticks;
        }

        /// <summary>
        /// ページ遷移用に以前のページに関わる情報をクリアする
        /// </summary>
        public virtual void ClearPrevInfo()
        {
            //title, macは保持する
            status = RunOrStop.NOOP;
            signalNum = 0;
            lastCycleTime = new CycleTime();
            productionHelper.list.Clear();
        }

        public enum RunOrStop { NOOP, RUN, STOP }
        public string getStatusString()
        {
            switch (status)
            {
                case RunOrStop.NOOP:
                    return "";
                case RunOrStop.RUN:
                    return "稼働中";
                case RunOrStop.STOP:
                    return "停止中";
            }
            return "";
        }
        public double getRunSec()
        {
            var isRunning = status == RunOrStop.RUN;

            return listProductionFactor
                .Where(q => q.status == ProductionFactor.Status.START_PRODUCTION
                         || q.status == ProductionFactor.Status.START_PRODUCTION_NOCT)
                .Select(q => q.getRunSec(isRunning)).Sum();
        }
        public string getRunSecStr(ProductionFactor factor = null)
        {
            return getSecString(getRunSec());
        }
        public string getSecString(double sec)
        {
            if (sec < 60)
            {
                return sec.ToString("F1")+"秒";
            }
            if (sec < 3600)
            {
                return (sec / 60).ToString("F1") + "分";
            }
            return (sec/3600).ToString("F1")+"時間";
        }

        public double getStopSec()
        {
            var isRunning = status == RunOrStop.RUN;

            return listProductionFactor
                .Where(q => q.status == ProductionFactor.Status.START_PRODUCTION
                         || q.status == ProductionFactor.Status.START_PRODUCTION_NOCT)
                .Select(q => q.getStopSec(isRunning)).Sum();
        }
        public string getStopSecStr(ProductionFactor factor = null)
        {
            return getSecString(getStopSec());
        }
        public long dekidaka { 
            get { return productionHelper.list.Select(q => q.dekidaka).Sum(); }
        }
        public double bekidou
        {
            get {
                var list = productionHelper.list.Where(q => q.status == ProductionFactor.Status.START_PRODUCTION
                                    || q.status == ProductionFactor.Status.START_PRODUCTION_NOCT);
                var bunbo = list.Select(q => q.dekidaka).Sum();
                if (bunbo == 0)
                {
                    return 0;
                }
                //各生産要因ごとの加重平均
                return list.Select(q => q.GetKadouritsu() * q.dekidaka).Sum()
                    / bunbo;
            }
        }


        public virtual void updateCycleTime(CycleTime ct)
        {
            if (ct == null)
            {
                lastCycleTime = null;
                return;
            }
            if (ct.btn == 0)
            {
                status = RunOrStop.STOP;
                signalNum++;
            }
            else
            {
                status = RunOrStop.RUN;
            }

            lastCycleTime = ct;

            //サイクルタイム、可動率計算
            productionHelper.update(ct, this);
        }
        public string GetMtRatio()
        {
            var list = this.listProductionFactor
                .Where(q => q.status == ProductionFactor.Status.START_PRODUCTION);
            var duration = list.Select(q => q.GetDurationSec()).Sum();
            if(duration==0) { return "-"; }
            var runSec = list.Select(q => q.runSec).Sum();

            return (100.0 * runSec / duration).ToString("F1") + "%";
        }

        public static PanelModel Create()
        {
            return new PanelModel();
        }

        #region 生産要因ごとの情報集計

        /// <summary>
        /// 生産要因情報の一括追加(Http経由)
        /// </summary>
        /// <param name="ary"></param>
        public void SetProductionFactor(ProductionFactor[] ary)
        {
            productionHelper.list.AddRange(ary);
            productionHelper.SortAndSetEndTicks();
        }
        /// <summary>
        /// 生産要因情報の単独追加(WebSocket経由)
        /// </summary>
        /// <param name="factor"></param>
        public void SetProductionFactor(ProductionFactor factor)
        {
            productionHelper.list.Add(factor);
            productionHelper.SortAndSetEndTicks();
        }

        public void RemoveProductionFactor(ProductionFactor factor)
        {
            productionHelper.list.Remove(factor);
        }

        public IReadOnlyList<ProductionFactor> listProductionFactor { 
            get
            {
                return productionHelper.list;
            } 
        }
        
        private class ProductionFactorHelper
        {
            public List<ProductionFactor> list { get; set; } = new List<ProductionFactor>();
            public const int SummaryDurationHour = 1;
            public void update(CycleTime cycle, PanelModel panel)
            {
                bool isTimeRangeHit = false;
                foreach(var factor in list)
                {
                    var ans = factor.updateByCycle(cycle);
                    isTimeRangeHit |= ans;
                }
                if(isTimeRangeHit) { return; }
                //集計時間枠より大きな信号は枠毎の出来高計算ができないため、
                //対象から除外する。また、前日からの継続信号破棄の効果も
                if ((cycle.ct01 + cycle.ct10) >= SummaryDurationHour * 3600)
                {
                    //最終シグナルがon->offなら信号数を減算
                    if(panel.status== RunOrStop.STOP)
                    {
                        panel.signalNum--;
                    }
                    return;
                }

                //未操作開始対応のため、CT無しの生産要因を追加する
                var stDt = cycle.dt.AddSeconds(-1 * (cycle.ct01 + cycle.ct10));
                var noct = new ProductionFactor()
                {
                    mac = cycle.mac,
                    status = ProductionFactor.Status.START_PRODUCTION_NOCT,
                    stTicks = stDt.Ticks,
                    endTicks = CreateEndTicks(cycle.dt),
                    isValid = ProductionFactor.Validation.VALID
                };
                panel.SetProductionFactor(noct);
                noct.updateByCycle(cycle);
            }



            public void SortAndSetEndTicks()
            {
                //時系列でソート
                list = list.OrderBy(q => q.stTicks).ToList();
                //ソート後に次の要因開始時刻を前の要因終了時刻に設定
                for (var i = 1; i < list.Count; i++)
                {
                    //long.MaxValueの時もしくは、NOCT時でより早い時間に
                    //休憩に入るなどの処理を行った場合
                    if(list[i - 1].endTicks > list[i].stTicks)
                    {
                        list[i - 1].endTicks = list[i].stTicks;
                    }
                }
                //最後以外は時刻計算を固定
                for(var i=0; i<list.Count-1; i++)
                {
                    list[i].isFixed = true;
                }
            }
        }
        #endregion

    }
}
