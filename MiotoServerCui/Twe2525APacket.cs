using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class Twe2525APacket : PacketCommon, IMonoPacket
    {
        /*
        :78811501A281021369000120000C4000080F3E3E43622685
         ^1^2^3^4^5^^^^^^^6^7^^^8^9^^^a^b^c^de1e2e3e4ef^g

        データフォーマット
         1: 送信元の論理デバイスID (0x78 は子機からの通知)
         2: コマンド
         3: パケット識別子 (アプリケーションIDより生成される)
         4: プロトコルバージョン (0x01 固定)
         5: LQI値、電波強度に応じた値で 0xFF が最大、0x00 が最小
         6: 送信元の個体識別番号
         7: 宛先の論理デバイスID
         8: タイムスタンプ
         9: 中継フラグ(中継済みなら1)
         a: 電源電圧[mV]
         b: 未使用
         c: TWELITE 2525Aの状態ビット。
         d: 未使用
         e1～e3: X,Y,Z軸の加速度値。
         ef: e1～e4の補正値　（LSBから順に２ビットずつ補正値、LSB側が　e1, MSB側が e4）
         g: チェックサム
         */
        public Twe2525APacket(uint mac, int lqi, int battX10, long ticks, int state)
        {
            this.mac = mac;
            this.lqi = (byte)lqi;
            this.batt = (float)(battX10 / 10.0);
            this.dt = new DateTime(ticks);
            this.state = (byte)state;
        }
        public Twe2525APacket()
        {

        }

        public float batt { get; private set; } = 0;
        public float gx { get; private set; } = 0;
        public float gy { get; private set; } = 0;
        public float gz { get; private set; } = 0;

        public int state { get; private set; } = 0;

        public bool parse(string msg, ref int ofs)
        {
            var ofsBackup = ofs;
            try
            {
                var logicDevId = read1Byte(msg, ref ofs);
                if (logicDevId != 0x78) { goto FALSE; }
                var cmd = read1Byte(msg, ref ofs);
                if (cmd != 0x81) { goto FALSE; }
                var packetId = read1Byte(msg, ref ofs);
                var protocolVer = read1Byte(msg, ref ofs);
                if (protocolVer != 0x01) { goto FALSE; }
                this.lqi = read1Byte(msg, ref ofs);
                this.mac = read4Byte(msg, ref ofs);
                var destLogicId = read1Byte(msg, ref ofs);
                var timeStamp = read2Byte(msg, ref ofs);
                var relayFlg = read1Byte(msg, ref ofs);

                var tmp = read2Byte(msg, ref ofs);
                this.batt = (float)tmp / 1000;

                tmp = read1Byte(msg, ref ofs);//未使用

                this.state = read1Byte(msg, ref ofs);//状態ビット

                tmp = read1Byte(msg, ref ofs);//未使用

                var x = read1Byte(msg, ref ofs);
                var y = read1Byte(msg, ref ofs);
                var z = read1Byte(msg, ref ofs);
                tmp = read1Byte(msg, ref ofs);//仕様書不在e4

                var minerbit = read1Byte(msg, ref ofs);//補正値は使用せず

                this.gx = getG(x, minerbit, 0);
                this.gy = getG(y, minerbit, 2);
                this.gz = getG(z, minerbit, 4);


                //2の補数
                byte sum = 0;
                for (var i = ofsBackup; i < ofs;)
                {
                    sum += read1Byte(msg, ref i);
                }
                sum = (byte)(0x100 - sum);
                var checksum = read1Byte(msg, ref ofs);
                if (sum != checksum) { goto FALSE; }

                this.dt = DateTime.Now;
            }catch(Exception ee)
            {
                goto FALSE;
            }

            return true;

        FALSE:
            ofs = ofsBackup;
            return false;

        }

        private float getG(byte main, byte miner, int ofs)
        {
            //00100110
            var bit = 0x03 & (miner >> ofs);
            double bunshi = (((double)main * 4 + (double)bit) * 4d) * 8d;
            double mg = bunshi / 5d - 1600d;

            return (float)mg/1000f;
        }

        public override string ToString()
        {
            return String.Format("mac:{0:X} lqi:{1:X} g:{2:F2}/{3:F2}/{4:F2} state:0x{5:X}", mac, lqi, gx, gy, gz, state);
        }

        public void d(string msg)
        {
            Console.WriteLine(msg);
        }

        public void registDb(DbWrapper db)
        {
            var packet = db.getLatestTwe2525PacketByMac(this.mac);
            if((packet!=null) && (packet.state == state)) { return; }//[TODO]要因登録時は有効に。トラッキング時は無効に
            db.insertOrUpdatePacket(this);

        }

        public string toCsv()
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss") + string.Format(",{0:x},{1:D},{2:F1},{3:D}",
                mac, lqi, batt, state);
        }
    }
}
