using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.CfgOption
{
    public class SerialCurrent
    {
        public long mac { get; set; }
        public string name { get; set; }
        
        public List<ChInfo> listChInfo { get; set; }

        public SerialCurrent()
        {
            listChInfo = new List<ChInfo>();
        }

        public class ChInfo
        {
            public float thresholdLtoH { get; set; } = 10.0f;
            public float thresholdHtoL { get; set; } = 5.0f;
            /// <summary>
            /// サイクル起動中の電流値一時低下を無視する時間(ミリ秒)
            /// </summary>
            public int avoidStopDetectionMsec { get; set; } = 3000;
        }
    }
}
