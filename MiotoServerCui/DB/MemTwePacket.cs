using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class MemTwePacket
    {
        [PrimaryKey]
        public long macAndBtn { get; set; }

        public long mac { get; set; }

        public byte btn { get; set; }

        public long ticks { get; set; }

        public int intervalMs { get; set; } = INTERVAL_UNDEF;//点滅対策を想定。対策は未実装

        public const int INTERVAL_UNDEF = 0;
        public static MemTwePacket Convert(TwePacket packet, MemTwePacket pre=null)
        {
            var twe = new MemTwePacket();
            twe.ticks = packet.dt.Ticks;
            twe.mac = packet.mac;
            twe.btn = packet.btn;
            twe.macAndBtn = (packet.mac << 4) + packet.btn;
            if (pre != null)
            {
                if(twe.btn == pre.btn)
                {
                    //ボタン変化が無い場合は以前のticksを用いる(0_00, 0_11等の算出のため)
                    twe.ticks = pre.ticks;
                }
                twe.intervalMs = (int)(packet.dt - (new DateTime(pre.ticks))).TotalMilliseconds;
            }
            return twe;
        }        
    }
}
