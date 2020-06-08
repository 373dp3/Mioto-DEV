using System;
using System.Collections.Generic;
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
            return ts.ToString();
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
            return ts.ToString();
        }
        public long dekidaka { get; private set; } = 0;
        public CycleTime lastCycleTime { get; private set; } = null;
        public long mac { get; set; } = 0;

        public void updateCycleTime(CycleTime ct)
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
                dekidaka++;
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
        }
        public string getBekidouStr()
        {
            var sum = stopSec + runSec;
            if (sum == 0) { return "-"; }
            return (100.0d * runSec / sum).ToString("F1") + " %";
        }
    }
}
