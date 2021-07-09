using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoBlazorCommon.Struct
{
    public class ConfigGanttPanel
    {
        public long dtFirstTicks { get; set; } = 0;
        [Required]
        [StringLength(50, ErrorMessage = "文字数が超過しています(50バイトまで)")]
        public string name { get; set; } = "";
        public List<Mac2Index> listMac2Index { get; set; } = new List<Mac2Index>();

        public string fontSize { get; set; } = "16px";

        public string graphHeight { get; set; } = "10";

        public string pollingSec { get; set; } = Config.POLLING_LONGPOLLING.ToString();

        public bool isItemCounts { get; set; } = true;//出来高

        public bool isBekidou { get; set; } = true;//可動率

        public bool isRunSec { get; set; } = true;//稼働時間秒

        public bool isStopSec { get; set; } = true;//停止時間秒

        public bool isCt { get; set; } = true; //最終CT

    }
}
