using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoServer;
using MiotoServer.Query;

namespace TwePacketTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string p1 = ":80000000240005810C490D01808205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012BC8";
            const string twe = ":7FD30101A88100763A008005000C9B0000014513260DEF2C";

            //従来型TWE CT拡張
            {
                var msg = "";
                int ofs = 0;
                msg = twe;
                ofs = 0;
                ofs = parseHeader(msg, ofs);
                var packet = new TwePacket(msg, ref ofs);
                var csv = packet.ToCSV();
            }


            //TWE-Lite PALパケット
            /*  
             *  実装方針
             *  ・シリアル受信メッセージ中、解析対象とする:までは上位で判定
             *  ・TwePALのインスタンスは:から後を示すofsを受け取り、フォーマット確認
             *  ・check メソッドを実装し、フォーマットに問題があればfalseを返す。
             * 
             * */
            {
                int ofs = 0;
                ofs = searchCollon(p1, ofs);

                var pal = new TwePalSensePacket();

                Assert.AreEqual(true, pal.parse(p1, ref ofs));
                Assert.AreEqual(0x24, pal.lqi);
                Assert.AreEqual(0x810C490D, pal.mac);
                Assert.AreEqual(true,
                    pal.listValueAndAttr.Contains(
                        string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08)));
                Assert.AreEqual(true,
                    pal.listValueAndAttr.Contains(
                        string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01)));
                Assert.AreEqual(true,
                    pal.listValueAndAttr.Contains(
                        string.Format("{0:F1},{1:D}", (0x0BA7) / 100, 0x01)));
                Assert.AreEqual(true,
                    pal.listValueAndAttr.Contains(
                        string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02)));
                Assert.AreEqual(true,
                    pal.listValueAndAttr.Contains(
                        string.Format("{0:D},{1:D}", 0x00000001, 0x03)));

                var ts = pal.dt - DateTime.Now;
                Assert.AreEqual(true, (Math.Abs(ts.TotalSeconds)<10));

                var list= pal.toCsvList();
                Assert.AreEqual(5, list.Count);
                var hitCnt = 0;
                foreach(var csv in list)
                {
                    if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08))) hitCnt++;
                    if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01))) hitCnt++;
                    if (csv.Contains(string.Format("{0:F1},{1:D}", (0x0BA7) / 100, 0x01))) hitCnt++;
                    if (csv.Contains(string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02))) hitCnt++;
                    if (csv.Contains(string.Format("{0:D},{1:D}", 0x00000001, 0x03))) hitCnt++;
                    Assert.AreEqual(true, csv.Contains("810C490D".ToLower()));
                }

                Assert.AreEqual(5, hitCnt);
            }
        }

        [TestMethod]
        public void TestOk()
        {
            const string p1 = ":80000000240005810C490D01808205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012BC8";
            int ofs = 0;
            ofs = searchCollon(p1, ofs);

            var pal = new TwePalSensePacket();

            Assert.AreEqual(true, pal.parse(p1, ref ofs));
            Assert.AreEqual(0x24, pal.lqi);
            Assert.AreEqual(0x810C490D, pal.mac);
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D}", (0x0BA7) / 100, 0x01)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:D},{1:D}", 0x00000001, 0x03)));

            var ts = pal.dt - DateTime.Now;
            Assert.AreEqual(true, (Math.Abs(ts.TotalSeconds) < 10));

            var list = pal.toCsvList();
            Assert.AreEqual(5, list.Count);
            var hitCnt = 0;
            foreach (var csv in list)
            {
                if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D}", (0x0BA7) / 100, 0x01))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02))) hitCnt++;
                if (csv.Contains(string.Format("{0:D},{1:D}", 0x00000001, 0x03))) hitCnt++;
                Assert.AreEqual(true, csv.Contains("810C490D".ToLower()));
            }

            Assert.AreEqual(5, hitCnt);
        }

        [TestMethod]
        public void TestOkNegativeValue()
        {
            string msg = string.Format(":80000000240005810C490D01808205113008020BCC1130010203AC05010002{0:X4}010200021C7F02030004000000012B", (short)-100);
            msg += buildCheckSum(msg);
            int ofs = 0;
            ofs = searchCollon(msg, ofs);

            var pal = new TwePalSensePacket();

            Assert.AreEqual(true, pal.parse(msg, ref ofs));
            Assert.AreEqual(0x24, pal.lqi);
            Assert.AreEqual(0x810C490D, pal.mac);
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D}", Convert.ToInt16(string.Format("{0:X4}",(short)-100),16) / 100, 0x01)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02)));
            Assert.AreEqual(true,
                pal.listValueAndAttr.Contains(
                    string.Format("{0:D},{1:D}", 0x00000001, 0x03)));

            var ts = pal.dt - DateTime.Now;
            Assert.AreEqual(true, (Math.Abs(ts.TotalSeconds) < 10));

            var list = pal.toCsvList();
            Assert.AreEqual(5, list.Count);
            var hitCnt = 0;
            foreach (var csv in list)
            {
                if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D}", Convert.ToInt16(string.Format("{0:X4}", (short)-100), 16) / 100, 0x01))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02))) hitCnt++;
                if (csv.Contains(string.Format("{0:D},{1:D}", 0x00000001, 0x03))) hitCnt++;
                Assert.AreEqual(true, csv.Contains("810C490D".ToLower()));
            }

            Assert.AreEqual(5, hitCnt);
        }

        [TestMethod]
        public void TestDb()
        {
            const string p1 = ":80000000240005810C490D01808205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012BC8";
            int ofs = 0;
            ofs = searchCollon(p1, ofs);

            var pal = new TwePalSensePacket();

            Assert.AreEqual(true, pal.parse(p1, ref ofs));

            var list = pal.toCsvList();
            Assert.AreEqual(5, list.Count);
            var hitCnt = 0;
            foreach (var csv in list)
            {
                if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", 3020 / 1000, 0x30, 0x08))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D},{2:D}", (0x03AC) / 1000, 0x30, 0x01))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D}", (0x0BA7) / 100, 0x01))) hitCnt++;
                if (csv.Contains(string.Format("{0:F1},{1:D}", (0x1C7F) / 100, 0x02))) hitCnt++;
                if (csv.Contains(string.Format("{0:D},{1:D}", 0x00000001, 0x03))) hitCnt++;
                Assert.AreEqual(true, csv.Contains("810C490D".ToLower()));
            }

            Assert.AreEqual(5, hitCnt);

            var tmpDbPath = "TestDb";
            if (Directory.Exists(tmpDbPath))
            {
                Directory.Delete(tmpDbPath, true);
            }
            var p = DbWrapper.getInstance(tmpDbPath);
            var param = new Param("http://localhost/pal");
            ParamTypeVolume filter = new ParamTypeVolume();
            filter.update(param);
            Assert.AreEqual(param.type, Param.TYPE.PAL);

            pal.registDb(p);

            Program.config = new System.Collections.Generic.Dictionary<string, string>();
            Program.config[Program.HM_KEY] = "800";
            DbChecker.getInstance(Program.config[Program.HM_KEY]);
            var ans = p.getCsv(param);
            var lines = Regex.Split(ans, @"[\r\n]{1,2}", RegexOptions.Multiline);
            Assert.AreEqual(5, lines.Length);

        }


        [TestMethod]
        public void TestErrorCheckSum()
        {
            const string p1 = ":80000000240005810C490D01808205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012BCA";
            int ofs = 0;
            ofs = searchCollon(p1, ofs);

            var pal = new TwePalSensePacket();
            int backupOfs = ofs;
            Assert.AreEqual(false, pal.parse(p1, ref ofs));
            Assert.AreEqual(backupOfs, ofs);
        }
        [TestMethod]
        public void TestErrorTwe()
        {
            const string twe = ":7FD30101A88100763A008005000C9B0000014513260DEF2C";
            int ofs = 0;
            ofs = searchCollon(twe, ofs);

            var pal = new TwePalSensePacket();
            int backupOfs = ofs;
            Assert.AreEqual(false, pal.parse(twe, ref ofs));
            Assert.AreEqual(backupOfs, ofs);
        }
        [TestMethod]
        public void TestErrorRouter()
        {
            string msg = ":80010000240005810C490D01808205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012B";
            msg += buildCheckSum(msg);
            int ofs = 0;
            ofs = searchCollon(msg, ofs);

            var pal = new TwePalSensePacket();
            int backupOfs = ofs;
            Assert.AreEqual(false, pal.parse(msg, ref ofs));
            Assert.AreEqual(backupOfs, ofs);
        }
        [TestMethod]
        public void TestErrorSensorNumOver()
        {
            string msg = ":80000000240005810C490D01808206113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012B";
            msg += buildCheckSum(msg);
            int ofs = 0;
            ofs = searchCollon(msg, ofs);

            var pal = new TwePalSensePacket();
            int backupOfs = ofs;
            Assert.AreEqual(false, pal.parse(msg, ref ofs));
            Assert.AreEqual(backupOfs, ofs);
        }

        [TestMethod]
        public void TestErrorSensorNumLesser()
        {
            string msg = ":80000000240005810C490D01808204113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012B";
            msg += buildCheckSum(msg);
            int ofs = 0;
            ofs = searchCollon(msg, ofs);

            var pal = new TwePalSensePacket();
            int backupOfs = ofs;
            Assert.AreEqual(false, pal.parse(msg, ref ofs));
            Assert.AreEqual(backupOfs, ofs);
        }
        [TestMethod]
        public void TestErrorFix80()
        {
            string msg = ":80000000240005810C490D01818205113008020BCC1130010203AC050100020BA7010200021C7F02030004000000012B";
            msg += buildCheckSum(msg);
            int ofs = 0;
            ofs = searchCollon(msg, ofs);

            var pal = new TwePalSensePacket();
            int backupOfs = ofs;
            Assert.AreEqual(false, pal.parse(msg, ref ofs));
            Assert.AreEqual(backupOfs, ofs);
        }

        private string buildCheckSum(string msg)
        {
            byte sum = 0;
            int ofs = 0;
            ofs = searchCollon(msg, ofs);

            for (var i = ofs; i < msg.Length;)
            {
                sum += read1Byte(msg, ref i);
            }
            sum = (byte)(0x100 - sum);
            return string.Format("{0:X2}", sum);
        }

        private int parseHeader(string msg, int ofs)
        {
            //原点調査
            ofs = searchCollon(msg, ofs);

            var preOrg = ofs;
            var h4byte = read4Byte(msg, ref preOrg);
            if (h4byte == 0x80000000)
            {
                throw new FormatException("TWE-Lite pal");
            }

            //解釈(48文字)
            if (((msg.Length - ofs) != 49) && ((msg.Length - ofs) != 48))
            {
                Program.d("length no match:" + (msg.Length - ofs) + " " + msg);
                Program.d("ofs:" + (ofs));
            }
            ofs += 2;//DST;
            byte msgType = read1Byte(msg, ref ofs);


            byte stype = read1Byte(msg, ref ofs);
            if ((stype != 0x01) && (stype != 0x15))
            {
                throw new FormatException("packet stype " + stype + " is not implimented. >> " + msg);
            }


            return ofs;
        }



        private static int searchCollon(string msg, int ofs)
        {
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == ':')
                {
                    ofs = i + 1;
                    break;
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
        protected UInt32 read4Byte(string msg, ref int ofs)
        {
            string str = readString(msg, ref ofs, 4);
            return Convert.ToUInt32(str, 16);
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

    }
}
