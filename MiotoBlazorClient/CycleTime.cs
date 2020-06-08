using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class CycleTime
    {
        //date time,mac,seq,btn,batt,lqi,0_00,0_01,0_10,0_11,DI
        public DateTime dt { get; set; }
        public DateTime createDt { get; set; }
        public long mac { get; set; }
        public byte seq { get; set; }
        public byte btn { get; set; }
        public float batt { get; set; }
        public byte lqi { get; set; }
        public double ct00 { get; set; }
        public double ct01 { get; set; }
        public double ct10 { get; set; }
        public double ct11 { get; set; }
        public byte DI { get; set; } = 0;

        public const double CT_NODATA = 0.0d;
        public static CycleTime Parse(string msg)
        {
            var ans = new CycleTime();
            ans.createDt = DateTime.Now;
            var csvAry = msg.Split(',');
            if(csvAry.Length<9) { return null; }
            int i = 0;
            ans.dt = DateTime.Parse(csvAry[i]); i++;
            ans.mac = Convert.ToInt64(csvAry[i],16); i++;
            ans.seq = Convert.ToByte(csvAry[i]); i++;
            ans.btn = Convert.ToByte(csvAry[i]); i++;
            ans.batt = (float)Convert.ToDouble(csvAry[i]); i++;
            ans.lqi = Convert.ToByte(csvAry[i]); i++;
            ans.ct00 = checkAndSetCt(csvAry, i); i++;
            ans.ct01 = checkAndSetCt(csvAry, i); i++;
            ans.ct10 = checkAndSetCt(csvAry, i); i++;
            ans.ct11 = checkAndSetCt(csvAry, i); i++;
            try
            {
                ans.DI = Convert.ToByte(csvAry[i]); i++;
            }catch(Exception e) { i++; }
            return ans;
        }
        private static double checkAndSetCt(string[] csvAry, int i)
        {
            try
            {
                return Convert.ToDouble(csvAry[i]);
            }catch(Exception e)
            {
                return CT_NODATA;
            }
        }
        private string ctMsg(double ct)
        {
            if (ct == CT_NODATA) { return ""; }
            return ct.ToString("F1");
        }

        public string ToString(int vector = 0)
        {
            var masc = (byte)(0x01 << vector);
            if((btn & masc) > 0)
            {
                return $"{mac.ToString("x8")},ON,{ctMsg(ct01)},{ctMsg(ct11)}";
            }
            else
            {
                return $"{mac.ToString("x8")},OFF,{ctMsg(ct10)},{ctMsg(ct00)}";
            }
        }
    }
}
