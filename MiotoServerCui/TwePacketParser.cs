/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class TwePacketParser
    {

        /*
            //TWE-Lite データフォーマット(一部dp3による拡張)
            :7FD30101A88100763A008005000C9B0000014513260DEF2C

            7F: 論理アドレス
            D3: コマンド(D3はdp3用拡張コマンド)
            01: センサー種別
            01: プロトコルバージョン
            A8: LQI
            8100763A: MAC
            00: 親子フラグ
            8005: タイムスタンプ
            00: 中継フラグ
            0C9B: バッテリ電圧
            00: SEQ
            00: btn
            01: Rep
            45: AD1
            13: AD2
            26: AD3
            0D: AD4
            EF: ADBits1~4
            2C: LRC
         */

        private List<IMonoPacket> listPacketFilter;
        public TwePacketParser()
        {
            listPacketFilter = new List<IMonoPacket>();
            listPacketFilter.Add(new TwePalSensePacket());
            listPacketFilter.Add(new Twe2525APacket());
        }

        public void parse(string msg)
        {
            parse(msg, 4);
        }
        public void parse(string msg, int btnBitLength)
        {
            int ofs = 0;
            //原点調査
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == ':')
                {
                    ofs = i + 1;
                    break;
                }
            }

            //TWE-Lite PAL以降の新しいパケットフォーマットに対する処理
            var p = DbWrapper.getInstance();
            foreach (var filter in listPacketFilter)
            {
                if(filter.parse(msg, ref ofs) == false) {
                    continue;
                }
                filter.registDb(p);
                Program.d(filter.ToString());
                return;
            }

            //以降、従来型のパケット処理

            //解釈(48文字)
            if (((msg.Length - ofs) != 49) && ((msg.Length - ofs) != 48))
            {
                Program.d("length no match:" + (msg.Length - ofs) + " " + msg);
                Program.d("ofs:" + (ofs));
            }
            ofs += 2;//DST;
            byte msgType = read1Byte(msg, ref ofs);
            //Console.WriteLine("msg type: 0x" +Convert.ToString(msgType, 16));
            byte stype = read1Byte(msg, ref ofs);
            if ((stype != 0x01) && (stype != 0x15))
            {
                Program.d("packet stype " + stype + " is not implimented. >> " + msg);
                return;
            }
            switch (msgType)
            {
                case 0xD3:
                    ofs = doTweCtPacket(msg, btnBitLength, ofs);
                    break;
                default:
                    var wrapper = DbWrapper.getInstance();
                    var packet = new TwePacket(msg, ref ofs);
                    var csv = packet.ToCSV();
                    wrapper.insertCsv(packet, csv);
                    Program.d("twe:" + csv);
                    break;
            }
        }

        private int doTweCtPacket(string msg, int btnBitLength, int ofs)
        {
            TweCtPacket packet = new TweCtPacket(msg, ref ofs);
            var wrapper = DbWrapper.getInstance();
            List<TweCtPacket> list = wrapper.getLatestTweCtPacketByMac(packet.mac);

            if ((list.Count == 0) || (list[0].seq != packet.seq))
            {
                wrapper.insertOrUpdatePacket(packet);
                string deltaSec = "";
                if (list.Count != 0)
                {
                    deltaSec = packet.getTimeSpanSec(list[0]).ToString("0.0");
                }
                var isInsert = false;
                for (int i = 0; i < btnBitLength; i++)
                {
                    string csv = "";
                    bool flg = packet.isOn(i);
                    TweCtPacket flip = null;
                    TweCtPacket again = null;
                    for (int j = 0; j < list.Count; j++)
                    {
                        //フラグが反転したパケット
                        if ((flip == null) && (flg != list[j].isOn(i)))
                        {
                            flip = list[j];
                        }

                        //直前の情報と比較してビットに変化がなければ
                        //時間間隔を計算しない。
                        if ((j == 0) && (flip == null)) { break; }

                        //反転後、再びフラグが復帰したパケット
                        if ((flip != null) && (flg == list[j].isOn(i)))
                        {
                            again = list[j];
                            break;
                        }
                    }
                    if ((flip != null) && (again != null))
                    {
                        if (flg)
                        {
                            csv += ",," + packet.getTimeSpanSec(flip).ToString("0.0") + ",," + packet.getTimeSpanSec(again).ToString("0.0");
                        }
                        else
                        {
                            csv += "," + packet.getTimeSpanSec(again).ToString("0.0") + ",," + packet.getTimeSpanSec(flip).ToString("0.0") + ",";
                        }
                        //date,mac,seq,btn,bat,lqi,on/off
                        csv = packet.ToCSV(flip.seq) + csv;
                        if (i>0) { csv += "," + (i + 1); }
                        Program.d("Th:" + System.Threading.Thread.CurrentThread.ManagedThreadId + " " + csv);
                        wrapper.insertCsv(packet, csv);
                        isInsert = true;
                    }
                }//flg 0-btnBitLength loop

                //プレス機等、単エッジ信号用の処理 btnは常に0、seqのみ異なる信号
                if ((isInsert == false) && (list[0].btn == packet.btn) && (packet.btn == 0))
                {
                    var csv = "," + packet.getTimeSpanSec(list[0]).ToString("0.0") + ",,,";
                    csv = packet.ToCSV(list[0].seq) + csv;
                    Program.d("Th(S):" + System.Threading.Thread.CurrentThread.ManagedThreadId + " " + csv);
                    wrapper.insertCsv(packet, csv);
                }
            }

            return ofs;
        }

        protected byte read1Byte(string msg, ref int ofs)
        {
            string str = "" + msg[ofs] + msg[ofs + 1];
            ofs += 2;
            return Convert.ToByte(str, 16);
        }
    }
}
