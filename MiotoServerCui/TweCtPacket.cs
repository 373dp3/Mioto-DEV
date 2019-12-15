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
    public class TweCtPacket : PacketCommon
    {
        public TweCtPacket(string msg, ref int ofs)
        {
            read1Byte(msg, ref ofs);//pver
            lqi = read1Byte(msg, ref ofs);
            mac = read4Byte(msg, ref ofs);
            ofs += 2;//parent flg
            ofs += 4;//twe time stamp
            ofs += 2;//relay flg
            batt = read2Byte(msg, ref ofs);
            seq = read1Byte(msg, ref ofs);
            btn = read1Byte(msg, ref ofs);
            byte rep = read1Byte(msg, ref ofs);
            dt = DateTime.Now.AddMilliseconds(-200*rep);
        }

        public TweCtPacket(UInt32 mac, byte seq, byte btn, byte lqi, UInt16 batt, long ticks)
        {
            this.mac = mac;
            this.seq = seq;
            this.btn = btn;
            this.lqi = lqi;
            this.batt = batt;
            this.dt = new DateTime(ticks);
        }

        public UInt16 batt { get; }
        public byte seq { get; }
        public byte btn { get; }
        
        public bool isOn(int bit)
        {
            byte flg = (byte)(1 << bit);
            if((btn & flg)>0)
            {
                return true;
            }
            return false;
        }
        public double getTimeSpanSec(TweCtPacket packet)
        {
            return ((dt.Ticks - packet.dt.Ticks) / 10000L)/1000.0d;
        }
        public override string ToString()
        {
            byte ms = (byte)(dt.Millisecond / 100);
            return "" + dt.ToShortDateString() + " " + dt.ToLongTimeString() + "." + ms +" "
                + " mac:" + Convert.ToString(mac, 16) + " seq:" + seq + " btn:" + btn + " batt:" + batt
                + " lqi:" + lqi;
        }
        public string ToCSV(int preSeq=0)
        {
            byte ms = (byte)(dt.Millisecond / 100);

            int diffSeq = 0;
            if(seq >= preSeq)
            {
                diffSeq = seq - preSeq;
            }
            else
            {
                diffSeq = seq + (256 - preSeq);
            }
            return "" + dt.ToString("yyyy/MM/dd HH:mm:ss")
                + "," + Convert.ToString(mac, 16) + "," + diffSeq + "," + btn + "," + (batt/1000d).ToString("0.0")
                + "," + lqi;

        }
    }
}
