using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MiotoServer.DB;
using System.ComponentModel;

namespace MiotoServer.Struct
{
    public class ConfigTwe
    {
        [Required]
        public long mac { get; set; } = 0;

        [Required]
        [StringLength(50, ErrorMessage = "文字数が超過しています(50バイトまで)")]
        public string name { get; set; } = "";

        public long Ticks { get; set; }

        public static ConfigTwe Convert(LastInfo info)
        {
            var ans = new ConfigTwe() { mac = info.mac, name = "", Ticks=info.ticks };
            return ans;
        }

        public static void Copy(ConfigTwe from, ConfigTwe to)
        {
            to.mac = from.mac;
            to.name = (string)from.name.Clone();
            to.Ticks = from.Ticks;
        }
            
    }
}
