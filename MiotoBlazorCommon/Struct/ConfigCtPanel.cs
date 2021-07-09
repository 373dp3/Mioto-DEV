using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MiotoBlazorCommon.Struct
{
    public class ConfigCtPanel
    {
        public long dtFirstTicks { get; set; } = 0;
        [Required]
        [StringLength(50, ErrorMessage = "文字数が超過しています(50バイトまで)")]
        public string name { get; set; } = "";
        public List<Mac2Index> listMac2Index { get; set; } = new List<Mac2Index>();

        public string fontSize { get; set; } = "16px";

        public string pollingSec { get; set; } = Config.POLLING_LONGPOLLING.ToString();

    }
    public class Mac2Index
    {
        public long mac { get; set; }
        public int index { get; set; }
    }

}
