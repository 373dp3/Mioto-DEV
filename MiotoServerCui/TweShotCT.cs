using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer
{
    /**
     * プレス機向けファームウェアをベースにしたCT判定用処理。
     * カウンタ回路を利用し5秒毎(変更可能)にカウント数をポーリングする。
     * 平面研削盤やフリッカータイプのシグナルタワーを想定。
     * */
    public class TweShotCT : PacketCommon, IMonoPacket
    {
        public float batt { get; private set; } = 0;
        public byte lqi { get; private set; } = 0;
        public string appKey { get; private set; } = "";
        public UInt16 seq { get; private set; } = 0;
        public UInt16 shots { get; private set; } = 0;
        public bool parse(string msg, ref int ofs)
        {
            if((msg.Length - ofs) < 48) { return false; }
            int ofsBackup = ofs;
            this.read1Byte(msg, ref ofs);//送信元論理アドレス
            this.read1Byte(msg, ref ofs);//コマンドID
            this.read1Byte(msg, ref ofs);//シーケンス番号
            mac = this.read4Byte(msg, ref ofs);//送信元MAC
            this.read4Byte(msg, ref ofs);//宛先MAC
            lqi = this.read1Byte(msg, ref ofs);//LQI
            this.read2Byte(msg, ref ofs);//ペイロード長
            appKey += (char)this.read1Byte(msg, ref ofs);//appKey bytes
            appKey += (char)this.read1Byte(msg, ref ofs);//appKey bytes
            appKey += (char)this.read1Byte(msg, ref ofs);//appKey bytes
            appKey += (char)this.read1Byte(msg, ref ofs);//appKey bytes
            if(appKey.CompareTo("SHT1")!=0)
            {
                goto FALSE;
            }
            seq = this.read2Byte(msg, ref ofs);//SHOT用シーケンス番号
            shots = this.read2Byte(msg, ref ofs);//ショット数
            batt = (float)(0.1 * this.read1Byte(msg, ref ofs));//電圧

            //2の補数法による確認
            byte sum = 0;
            for (var i = ofsBackup; i < ofs;)
            {
                sum += read1Byte(msg, ref i);
            }
            sum = (byte)(0x100 - sum);
            var checksum = read1Byte(msg, ref ofs);
            if (sum != checksum)
            {
                goto FALSE;
            }

            return true;

        FALSE:
            ofs = ofsBackup;
            return false;
        }
        public TwePacket convertToTwePacket()
        {
            byte btn = (byte)(this.shots > 0 ? 1 : 0);
            var twe = new TwePacket(mac, btn, lqi, batt);
            return twe;
        }

        public void registDb(DbWrapper db)
        {
            //標準ファームウェアを用いたCT処理用TWEをベースにする。
            var wrapper = DbWrapper.getInstance();
            var packet = convertToTwePacket();
            if(packet == null) { return; }
            var csv = packet.ToCSV();
            wrapper.insertCsv(packet, csv);
        }

        public string toCsv()
        {
            var twe = convertToTwePacket();
            if(twe == null) { return ""; }
            return twe.ToCSV();
        }

        public override string ToString()
        {
            return convertToTwePacket().ToString();
        }

    }
}
