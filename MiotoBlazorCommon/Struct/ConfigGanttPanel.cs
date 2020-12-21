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
    }
}
