/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class TwePacket : PacketCommon
    {
        public List<ushort> adList { get; }
        public TwePacket(string msg, ref int ofs)
        {
            read1Byte(msg, ref ofs);//pver
            lqi = read1Byte(msg, ref ofs);
            mac = read4Byte(msg, ref ofs);
            ofs += 2;//parent flg
            ofs += 4;//twe time stamp
            ofs += 2;//relay flg
            batt = read2Byte(msg, ref ofs);
            ofs += 2;//Mono独自温度(不正確)
            btn = read1Byte(msg, ref ofs);//ボタン
            btnCange = read1Byte(msg, ref ofs);//ボタン変化フラグ
            //10bit ADコンバータ
            adList = new List<ushort>();
            adList.Add((ushort)(read1Byte(msg, ref ofs) << 2));//AD1
            adList.Add((ushort)(read1Byte(msg, ref ofs) << 2));//AD2
            adList.Add((ushort)(read1Byte(msg, ref ofs) << 2));//AD3
            adList.Add((ushort)(read1Byte(msg, ref ofs) << 2));//AD4
            var lsb = read1Byte(msg, ref ofs);
            for (var i=0; i<4; i++)
            {
                var tmp = 0x02 & lsb;
                adList[i] += (ushort)tmp;
                lsb >>= 2;

            }
            //TWE-Liteにて電池電圧が有効な場合
            if((batt>1500) && (batt < 4000))
            {
                for(var i = 0; i < 4; i++)
                {
                    adList[i] = (ushort)((adList[i]<<2));//2ビットシフト(4倍)はTWE-Liteのソースコードより
                }
            }
            dt = DateTime.Now;
        }

        public UInt16 batt { get; }
        public byte btn { get; }
        public byte btnCange { get; }

        public bool isOn(int bit)
        {
            byte flg = (byte)(1 << bit);
            if ((btn & flg) > 0)
            {
                return true;
            }
            return false;
        }
        public double getTimeSpanSec(TweCtPacket packet)
        {
            return ((dt.Ticks - packet.dt.Ticks) / 10000L) / 1000.0d;
        }
        public override string ToString()
        {
            byte ms = (byte)(dt.Millisecond / 100);
            return "" + dt.ToShortDateString() + " " + dt.ToLongTimeString() + "." + ms + " "
                + " mac:" + Convert.ToString(mac, 16) + " btn:" + btn + " btnChg:" + btnCange + " batt:" + batt
                + " lqi:" + lqi + " ad1:" + adList[0] + " ad2:" + adList[1] + " ad3:" + adList[2] + " ad4:" + adList[3];
        }
        public string ToCSV()
        {
            byte ms = (byte)(dt.Millisecond / 100);
            return "" + dt.ToString("M/d HH:mm:ss")
                + "," + Convert.ToString(mac, 16) + "," + btn + "," + btnCange + "," + (batt / 1000d).ToString("0.0")
                + "," + lqi + "," + string.Join(",", adList);
        }

        public string ToCtCSV()
        {
            return "" + dt.ToString("yyyy/MM/dd HH:mm:ss")
                        + "," + Convert.ToString(mac, 16) + "," + 1 + "," + btn + "," + (batt / 1000d).ToString("0.0")
                        + "," + lqi;
        }

    }
}
