using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class TwePalSensePacket : PacketCommon, IMonoPacket
    {
        /*
:80000000240005810C490D01808205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012BC8

:
80000000　中継機MAC
24	　LQI
0005	　通し番号
810C490D　子機ID
01	　子機論理ID
80	　センサ種別(0x80固定)
82	　基板IDと接続基板(下位4bitがAMBであれば環境センサ)
05　	　センサデータ数

(1)
11	データ型(下位2bitがb01: short型、4bit目が1:拡張バイトの有無・有り)
30	データソース(0x30：電圧)
08	電源電圧：0x08
02	データ長
0BCC	データ 0x0BCC => 3020[mV]

　　(以下、これまでの繰り返し)

(2)
11	データ型(下位2bitがb01: short型、4bit目が1:拡張バイトの有無・有り)	
30	データソース(0x30：電圧)
01	ADC1:0x01
02	データ長
03AC	ADコンバータのデータ[mV]

(3)
05	(b101) bit10: int32型、bit2: 1は符号あり int32表記はバグ？
01	温度(0x01)
00	(拡張バイト未使用)
02	データ長(こちらの方が正しいみたい)
0BA7	29.83度

(4)
01
02	湿度(0x02)
00
02
1C7F	72.95%

(5)
02	bit10:11 => 可変長
03	照度
00
04	4バイト
00000001 lux

2B	チェックサム1(手前までのCRC8)
C8	チェックサム2(チェックサム1も含め、2の補数)
        */


        /*
:80000000A8001C82012B1E01808103113008020D0C1130010203E40000000101EC6E 
1	LQI	            A8	   168
2	続き番号	    001C	28
4	送信元のシリアルID	82012B1E	送信元のシリアルIDは2012B1E
1	送信元の論理デバイスID	1	送信元の論理デバイスIDは01
1	センサー種別	80	
1	PAL基板バージョンとPAL基板ID	81	開閉センサーパル	Ver.1
1	センサーデータの数	3	3つ
1	各種情報ビット値	11	拡張バイトあり	符号なしShort型
1	データソース	30	ADC
1	拡張バイト	    8	電源電圧
1	データ長	    2	2バイト
2	データ	        0D0C	3340ｍV
1	各種情報ビット値	11	拡張バイトあり	符号なしShort型
1	データソース	30	ADC
1	拡張バイト	    1	ADC1
1	データ長	    2	2バイト
2	データ	        30000	996mV
1	各種情報ビット値	0	拡張バイトなし	符号なしChar型
1	データソース	0	磁気
1	拡張バイト	    0	0
1	データ長	    1	1バイト
1	データ	        1	磁石(N極)が近くにある
1	チェックサム1	EC	
1	チェックサム2	6E	
         */

        //センサの値,種別コードの組み合わせ
        public List<string> listValueAndAttr { get; private set; } = new List<string>();
        public enum CODE { NOOP, MAGG, AMB, MOT } //PAL基板IDに準拠(並べ替え厳禁)

        const byte DIMM_MAGPOLE = 0x00;
        const byte DIMM_VOLT = 0x30;
        const byte DIMM_TEMP = 0x01;
        const byte DIMM_HUMI = 0x02;
        const byte DIMM_LUX = 0x03;



        public CODE code { get; private set; } = CODE.NOOP;
        public float batt { get; private set; } = 0;
        public float adc1 { get; private set; } = 0;
        public float temp { get; private set; } = 0;
        public float humi { get; private set; } = 0;
        public int lux { get; private set; } = 0;

        public const byte MAG_UNDEF = 0xff;
        public byte magPole { get; private set; } = MAG_UNDEF;

        public const byte SENSE_NODEF = 0x00;
        public const byte SENSE_MAG = 0x81;//開閉センサPAL
        public const byte SENSE_AMB = 0x82;//環境センサPAL
        byte senseType = SENSE_NODEF;

        public int seq { get; private set; } = 0;

        public bool parse(string msg, ref int ofs)
        {
            clear();
            var ofsBackup = ofs;
            long tmp = 0;

            try
            {
                tmp = read4Byte(msg, ref ofs);
                //中継パケットの場合、中継機のMACが入る。if (tmp != 0x80000000L) { goto FALSE; }

                this.lqi = read1Byte(msg, ref ofs);
                tmp = read2Byte(msg, ref ofs);      //通し番号
                seq = (int)tmp;
                this.mac = read4Byte(msg, ref ofs);
                tmp = read1Byte(msg, ref ofs);      //子機論理ID
                senseType = read1Byte(msg, ref ofs);      //センサ種別(0x80固定)
                if (senseType != 0x80) { goto FALSE; }
                senseType = read1Byte(msg, ref ofs);      //接続基板(0x82以外は未実装)
                if ((senseType != SENSE_MAG) && (senseType != SENSE_AMB))
                { goto FALSE; }    //0x82(AMB)以外は未実装

                switch (senseType)
                {
                    case SENSE_MAG:
                        code = CODE.MAGG;
                        break;
                    case SENSE_AMB:
                        code = CODE.AMB;
                        break;
                    default:
                        goto FALSE;//0x82(AMB)以外は未実装
                }

                var sensnum = read1Byte(msg, ref ofs);//センサデータ数

                for (var i = 0; i < sensnum; i++)
                {
                    var dtype = read1Byte(msg, ref ofs);//データ型
                    bool isSigned = (dtype & 0x04) > 0x00;//2bit目が1なら符号付き
                    var dimm = read1Byte(msg, ref ofs);//センサ計測値の次元
                    var exp = read1Byte(msg, ref ofs); //拡張バイト
                    var len = read1Byte(msg, ref ofs);//データ長
                    switch (len)
                    {
                        case 1:
                            if (isSigned)
                            {
                                tmp = readSigned1Byte(msg, ref ofs);
                            }
                            else
                            {
                                tmp = read1Byte(msg, ref ofs);
                            }
                            break;
                        case 2:
                            if (isSigned)
                            {
                                tmp = readSigned2Byte(msg, ref ofs);
                            }
                            else
                            {
                                tmp = read2Byte(msg, ref ofs);
                            }
                            break;
                        case 4:
                            if (isSigned)
                            {
                                tmp = readSigned4Byte(msg, ref ofs);
                            }
                            else
                            {
                                tmp = read4Byte(msg, ref ofs);
                            }
                            break;
                    }
                    var val = "";
                    switch (dimm)
                    {
                        case DIMM_MAGPOLE:
                            val = string.Format("{0:},{1:D}", tmp, dimm);
                            magPole = (byte)tmp;
                            break;
                        case DIMM_VOLT:
                            val = string.Format("{0:F1},{1:D},{2:D}", tmp / 1000, dimm, exp);
                            if (exp == 0x01) { this.adc1 = (float)tmp / 1000; }
                            if (exp == 0x08) { this.batt = (float)tmp / 1000; }
                            break;
                        case DIMM_TEMP:
                            val = string.Format("{0:F1},{1:D}", tmp / 100, dimm);
                            this.temp = (float)tmp / 100;
                            break;
                        case DIMM_HUMI:
                            val = string.Format("{0:F1},{1:D}", tmp / 100, dimm);
                            this.humi = (float)tmp / 100;
                            break;
                        case DIMM_LUX:
                            val = string.Format("{0:D},{1:D}", tmp, dimm);
                            this.lux = (int)tmp;
                            break;
                    }
                    this.listValueAndAttr.Add(val);
                }

                //チェックサム(CRC8は未実装、2の補数のみ)
                ofs += 2;//CRC8は読み飛ばす

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
            }
            catch (Exception e)
            {
                goto FALSE;
            }


            return true;

        FALSE:
            clear();
            ofs = ofsBackup;
            return false;
        }

        private void clear()
        {
            this.listValueAndAttr.Clear();
            this.lqi = 0;
            this.mac = 0;
        }


        public void registDb(DbWrapper db)
        {
            switch (code)
            {
                case CODE.AMB:
                    db.insertCsv(this);
                    break;
                case CODE.MAGG:
                    //[TODO] 時間間隔、登録処理
                    break;
            }
        }

        public List<string> toCsvList()
        {
            var ans = new List<string>();
            foreach (var item in this.listValueAndAttr)
            {
                var csv = "" + dt.ToString("yyyy/MM/dd HH:mm:ss")
                + "," + Convert.ToString(mac, 16) + "," + lqi + "," + item;
                ans.Add(csv);
            }

            return ans;
        }

        public string toCsv()
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss") + string.Format(",{0:x},{1:D},{2:F1},{3:F1},{4:F1},{5:D}",
                mac, lqi, batt, temp, humi, lux);
        }

        public override string ToString()
        {
            switch (code)
            {
                case CODE.AMB:
                    return string.Format("PAL mac:{0:X} lqi:{1:D} bat:{2:F1} t:{3:F1} h:{4:F1} lux:{5:D} seq:{6:D}",
                        mac, lqi, batt, temp, humi, lux, seq);
                case CODE.MAGG:
                    return string.Format("PAL mac:{0:X} lqi:{1:D} bat:{2:F1} ad:{3:F1} mag:{4:D} seq:{5:D}",
                        mac, lqi, batt, adc1, magPole, seq);
            }
            return "";
        }
    }
}
