using MiotoServer.DB;
using MiotoServer.Struct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class PanelModel
    {     
        public string title { get; set; }
        public enum RunOrStop { NOOP, RUN, STOP}
        public RunOrStop status { get; private set; } = RunOrStop.NOOP;
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
        public double runSec { get; set; } = 0;

        public string getRunSecStr()
        {
            var offset = 0.0d;
            if ((lastCycleTime != null) && (status == RunOrStop.RUN))
            {
                offset = (DateTime.Now - lastCycleTime.createDt).TotalSeconds;
            }
            var ts = new TimeSpan(0, 0, (int)(runSec + offset));
            return getSecString(ts);
        }

        public string getSecString(TimeSpan ts)
        {
            if (ts.TotalSeconds < 60)
            {
                return ts.ToString(@"m\:ss");
            }
            return ts.ToString(@"h\:mm\:ss");
        }

        public double stopSec { get; set; } = 0;
        public string getStopSecStr()
        {
            var offset = 0.0d;
            if ((lastCycleTime != null) && (status == RunOrStop.STOP))
            {
                offset = (DateTime.Now - lastCycleTime.createDt).TotalSeconds;
            }
            var ts = new TimeSpan(0, 0, (int)(stopSec + offset));
            return getSecString(ts);
        }
        public long signalNum { get; private set; } = 0;
        public long dekidaka { 
            get { return productionHelper.list.Select(q => q.dekidaka).Sum(); }
        }
        public double bekidou
        {
            get {
                return productionHelper.list.Select(q => q.GetKadouritsu() * q.dekidaka).Sum()
                    / productionHelper.list.Select(q => q.dekidaka).Sum();
            }
        }

        public CycleTime lastCycleTime { get; private set; } = null;
        public long mac { get; set; } = 0;

        public virtual void updateCycleTime(CycleTime ct)
        {
            var preStatus = status;
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
            switch (status)
            {
                case RunOrStop.RUN://01,11
                    stopSec += ct.ct01;
                    break;
                case RunOrStop.STOP://10,00
                    runSec += ct.ct10;
                    break;
            }

            //初回の信号は前日からの保持の可能性があるため積算対象から外す
            if(preStatus== RunOrStop.NOOP)
            {
                stopSec = 0;
                runSec = 0;
            }
            lastCycleTime = ct;

            //サイクルタイム、可動率計算
            productionHelper.update(ct);
        }
        public string getBekidouStr()
        {
            var sum = stopSec + runSec;
            if (sum == 0) { return "-"; }
            return (100.0d * runSec / sum).ToString("F1") + " %";
        }

        public static PanelModel Create()
        {
            return new PanelModel();
        }

        #region 生産要因ごとの情報集計
        private ProductionFactorHelper productionHelper = new ProductionFactorHelper();       

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


        private class ProductionFactorHelper
        {
            public List<ProductionFactor> list { get; set; } = new List<ProductionFactor>();
            public void update(CycleTime cycle)
            {
                foreach(var item in list)
                {
                    item.updateByCycle(cycle);
                }
            }

            public void SortAndSetEndTicks()
            {
                //時系列でソート
                list = list.OrderBy(q => q.stTicks).ToList();
                //ソート後に次の要因開始時刻を前の要因終了時刻に設定
                for (var i = 1; i < list.Count; i++)
                {
                    list[i - 1].endTicks = list[i].stTicks;
                }
            }
        }
        #endregion

    }
}
