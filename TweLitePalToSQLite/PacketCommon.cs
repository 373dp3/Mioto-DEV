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
    public class PacketCommon
    {
        public byte lqi { get; protected set; }
        public UInt32 mac { get; protected set; }
        public DateTime dt { get; protected set; }

        public double getTimeSpanSec(PacketCommon packet)
        {
            return ((dt.Ticks - packet.dt.Ticks) / 10000L) / 1000.0d;
        }

        protected byte read1Byte(string msg, ref int ofs)
        {
            string str = "" + msg[ofs] + msg[ofs + 1];
            ofs += 2;
            return Convert.ToByte(str, 16);
        }
        protected UInt16 read2Byte(string msg, ref int ofs)
        {
            string str = "" + msg[ofs] + msg[ofs + 1] + msg[ofs + 2] + msg[ofs + 3];
            ofs += 4;
            return Convert.ToUInt16(str, 16);
        }
        protected string readString(string msg, ref int ofs, int length)
        {
            string str = "";
            for (int i = 0; i < length * 2; i++)
            {
                str += msg[ofs + i];
            }
            ofs += length * 2;
            return str;
        }
        protected UInt32 read4Byte(string msg, ref int ofs)
        {
            string str = readString(msg, ref ofs, 4);
            return Convert.ToUInt32(str, 16);
        }

        protected sbyte readSigned1Byte(string msg, ref int ofs)
        {
            string str = readString(msg, ref ofs, 1);
            return Convert.ToSByte(str, 16);
        }
        protected Int16 readSigned2Byte(string msg, ref int ofs)
        {
            string str = readString(msg, ref ofs, 2);
            return Convert.ToInt16(str, 16);
        }
        protected Int32 readSigned4Byte(string msg, ref int ofs)
        {
            string str = readString(msg, ref ofs, 4);
            return Convert.ToInt32(str, 16);
        }

        protected void d(string msg)
        {
//            Program.d(msg);
        }
    }
}
