using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoServer;
using System.Data.SQLite;
using MiotoServer.Query;

namespace TwePacketTest
{
    [TestClass]
    public class UnitTestComSerialPacket
    {
        [TestMethod]
        public void Test正常系_通常()
        {
            const string packet = "[81003542:251] $CT10,0,3,11,0,0*b9";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);//呼び出し元全てに共通しているため、一度MAC後の:までシークする。

            var twe = new TweComSerialPacket();
            Assert.AreEqual(true, twe.parse(packet, ref ofs));
            Assert.AreEqual(0x81003542, twe.mac);
            Assert.AreEqual("0.0,0.3,1.1,0.0,0.0", twe.csv);
            Assert.AreEqual("ct10", twe.key);
        }
        [TestMethod]
        public void Test正常系_通常2()
        {
            const string packet = "[81003542:2] $CT10,3,1,32,0,0*b5AA";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);//呼び出し元全てに共通しているため、一度MAC後の:までシークする。

            var twe = new TweComSerialPacket();
            Assert.AreEqual(true, twe.parse(packet, ref ofs));
            Assert.AreEqual(0x81003542, twe.mac);
            Assert.AreEqual("0.3,0.1,3.2,0.0,0.0", twe.csv);
            Assert.AreEqual("ct10", twe.key);
        }

        [TestMethod]
        public void Test正常系_DB1()
        {
            DbWrapper.getInstance("TestDb");
            var db = DbComSerial.getInstance();
            var p = MiotoServerWrapper.getInstance(new Config());

            for (int i=0; i<5; i++)
            {
                string packet = "[81003542:2] $CT10,3,1,32,0,0*b5";
                int ofs = 0;
                ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
                var twe = new TweComSerialPacket();
                twe.parse(packet, ref ofs);
                db.insert(twe);
            }
            Thread.Sleep(1500);
            for (int i = 0; i < 5; i++)
            {
                string packet = "[81003524:2] $CT10,3,1,32,0,0*b5";
                int ofs = 0;
                ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
                var twe = new TweComSerialPacket();
                twe.parse(packet, ref ofs);
                db.insert(twe);
            }
            var param = new Param("");
            param.memDbKey = "ct10";
            var ans1 = db.getCsv(param);
            param.macList = new System.Collections.Generic.List<uint>();
            param.macList.Add(Convert.ToUInt32("81003542", 16));
            var ans2 = db.getCsv(param);
            Assert.AreNotEqual(ans1.Length, ans2.Length);
            Assert.AreNotEqual(0, ans1.Length);
            Assert.AreNotEqual(0, ans2.Length);
            /*
            var ans1 = db.getCsv();
            Assert.IsNotNull(ans1);
            db.purgeBySec(1);
            var ans2 = db.getCsv();
            var diff = ans1.Length - ans2.Length;
            Assert.AreNotEqual(ans1.Length, ans2.Length);
            //*/

        }

        [TestMethod]
        public void Test異常系_開始ブラケット無し()
        {
            const string packet = "81003542:251] $CT10,0,3,11,0,0*b9";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
            var twe = new TweComSerialPacket();
            Assert.AreEqual(false, twe.parse(packet, ref ofs));
        }

        [TestMethod]
        public void Test異常系_MAC桁不足()
        {
            const string packet = "[1003542:251] $CT10,0,3,11,0,0*b9";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
            var twe = new TweComSerialPacket();
            Assert.AreEqual(false, twe.parse(packet, ref ofs));
        }

        [TestMethod]
        public void Test異常系_チェックサム不足1()
        {
            const string packet = "[81003542:251] $CT10,0,3,11,0,0*b";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
            var twe = new TweComSerialPacket();
            Assert.AreEqual(false, twe.parse(packet, ref ofs));
        }

        [TestMethod]
        public void Test異常系_チェックサム不足2()
        {
            const string packet = "[81003542:251] $CT10,0,3,11,0,0*";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
            var twe = new TweComSerialPacket();
            Assert.AreEqual(false, twe.parse(packet, ref ofs));
        }

        [TestMethod]
        public void Test異常系_正規表現チェック()
        {
            {
                const string packet = "[81003542:251] $CT10,0,3,11,0,0*bZ";
                int ofs = 0;
                ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
                var twe = new TweComSerialPacket();
                Assert.AreEqual(false, twe.parse(packet, ref ofs));
            }
            {
                const string packet = "[8Z003542:251] $CT10,0,3,11,0,0*b9";
                int ofs = 0;
                ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
                var twe = new TweComSerialPacket();
                Assert.AreEqual(false, twe.parse(packet, ref ofs));
            }
            {
                const string packet = "[81003542:2510] $CT10,0,3,11,0,0*b9";
                int ofs = 0;
                ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);
                var twe = new TweComSerialPacket();
                Assert.AreEqual(false, twe.parse(packet, ref ofs));
            }
        }

        [TestMethod]
        public void Test異常系_チェックサム()
        {
            const string packet = "[81003542:251] $CT10,0,3,11,0,0*b8";
            int ofs = 0;
            ofs = UnitTestTwePacketBase.searchCollon(packet, ofs);

            var twe = new TweComSerialPacket();
            Assert.AreEqual(false, twe.parse(packet, ref ofs));
        }
    }
}
