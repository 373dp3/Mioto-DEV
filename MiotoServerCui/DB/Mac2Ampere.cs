using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer.DB
{
    public class Mac2Ampere
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

        public static void update(Mac2Ampere ins, TweComSerialPacket packet)
        {
            var span = new TimeSpan(packet.dt.Ticks - ins.ticks);
            var spanHours = span.TotalSeconds / 3600.0;
            ins.ticks = packet.dt.Ticks;
            ins.Ah1 += packet.values[0] * spanHours;
            ins.Ah2 += packet.values[1] * spanHours;
            ins.Ah3 += packet.values[2] * spanHours;
            ins.Ah4 += packet.values[3] * spanHours;
            ins.Ah5 += packet.values[4] * spanHours;

            ins.maxA1 = Math.Max(ins.maxA1, packet.values[0]);
            ins.maxA2 = Math.Max(ins.maxA2, packet.values[1]);
            ins.maxA3 = Math.Max(ins.maxA3, packet.values[2]);
            ins.maxA4 = Math.Max(ins.maxA4, packet.values[3]);
            ins.maxA5 = Math.Max(ins.maxA5, packet.values[4]);
        }

        public static void clear(Mac2Ampere ins)
        {
            ins.Ah1 = 0;
            ins.Ah2 = 0;
            ins.Ah3 = 0;
            ins.Ah4 = 0;
            ins.Ah5 = 0;

            ins.maxA1 = 0;
            ins.maxA2 = 0;
            ins.maxA3 = 0;
            ins.maxA4 = 0;
            ins.maxA5 = 0;
        }

        public static string ToCSV(Mac2Ampere ins)
        {
            var dtStr = (new DateTime(ins.ticks)).ToString("yyyy/MM/dd HH:mm:ss");
            return $"{dtStr},{ins.mac.ToString("x")},{ins.Ah1.ToString("F1")},{ins.Ah2.ToString("F1")},{ins.Ah3.ToString("F1")},{ins.Ah4.ToString("F1")},{ins.Ah5.ToString("F1")},"
                +$"{ins.maxA1.ToString("F1")},{ins.maxA2.ToString("F1")},{ins.maxA3.ToString("F1")},{ins.maxA4.ToString("F1")},{ins.maxA5.ToString("F1")}";
        }
    }
}
