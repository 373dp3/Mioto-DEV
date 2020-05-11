using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class MemMac2Ah
    {
        [PrimaryKey]
        public long mac { get; set; } = 0;
        public long ticks { get; set; } = 0;

        public double Ah1 { get; set; } = 0;
        public double Ah2 { get; set; } = 0;
        public double Ah3 { get; set; } = 0;
        public double Ah4 { get; set; } = 0;
        public double Ah5 { get; set; } = 0;

        public float maxA1 { get; set; } = 0;
        public float maxA2 { get; set; } = 0;
        public float maxA3 { get; set; } = 0;
        public float maxA4 { get; set; } = 0;
        public float maxA5 { get; set; } = 0;

        public static void update(MemMac2Ah ins, TweComSerialPacket packet)
        {
            var span = new TimeSpan(packet.dt.Ticks - ins.ticks);
            var spanHours = span.TotalSeconds / 3600.0;
            ins.Ah1 = packet.values[0] * spanHours;
            ins.Ah2 = packet.values[1] * spanHours;
            ins.Ah3 = packet.values[2] * spanHours;
            ins.Ah4 = packet.values[3] * spanHours;
            ins.Ah5 = packet.values[4] * spanHours;

            ins.maxA1 = Math.Max(ins.maxA1, packet.values[0]);
            ins.maxA2 = Math.Max(ins.maxA2, packet.values[1]);
            ins.maxA3 = Math.Max(ins.maxA3, packet.values[2]);
            ins.maxA4 = Math.Max(ins.maxA4, packet.values[3]);
            ins.maxA5 = Math.Max(ins.maxA5, packet.values[4]);
        }
    }
}
