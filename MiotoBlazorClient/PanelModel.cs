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
        public long mac { get; set; } = 0;

        public RunOrStop status { get; private set; } = RunOrStop.NOOP;
        public long signalNum { get; private set; } = 0;
        public CycleTime lastCycleTime { get; private set; } = new CycleTime();

        private ProductionFactorHelper productionHelper = new ProductionFactorHelper();

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

        public string getRunSecStr(ProductionFactor factor = null)
        {
            var sum = listProductionFactor
                .Where(q => q.status == ProductionFactor.Status.START_PRODUCTION)
                .Select(q => q.runSec).Sum();
            return getSecString(sum);
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

        public string getStopSecStr(ProductionFactor factor = null)
        {
            var sum = listProductionFactor
                .Where(q => q.status == ProductionFactor.Status.START_PRODUCTION)
                .Select(q => q.stopSec).Sum();
            return getSecString(sum);
        }
        public long dekidaka { 
            get { return productionHelper.list.Select(q => q.dekidaka).Sum(); }
        }
        public double bekidou
        {
            get {
                if(productionHelper.list.Select(q => q.dekidaka).Sum() == 0)
                {
                    return 0;
                }
                //各生産要因ごとの加重平均
                return productionHelper.list.Select(q => q.GetKadouritsu() * q.dekidaka).Sum()
                    / productionHelper.list.Select(q => q.dekidaka).Sum();
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
            productionHelper.update(ct);
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

        public IReadOnlyList<ProductionFactor> listProductionFactor { 
            get
            {
                return productionHelper.list;
            } 
        }
        
        private class ProductionFactorHelper
        {
            public List<ProductionFactor> list { get; set; } = new List<ProductionFactor>();
            public void update(CycleTime cycle)
            {
                foreach(var factor in list)
                {
                    factor.updateByCycle(cycle);
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
