using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoServer;
using System;

namespace TwePacketTest
{
    [TestClass]
    public class UnitTestTweShotCT
    {
        [TestMethod]
        public void TestMethod1()
        {
            var p = new TweShotCT();
            int i = 0;
            var msg = ":FECC068100763A0000000099000953485431000500221EF8";
            i = searchCollon(msg, i);
            var ans = p.parse(msg, ref i);
            Assert.AreEqual(true, ans);
            Assert.AreEqual(0x8100763A, p.mac);
            Assert.AreEqual(0x99, p.lqi);
            Assert.AreEqual("SHT1", p.appKey);
            Assert.AreEqual(5, p.seq);
            Assert.AreEqual(0x0022, p.shots);
            Assert.AreEqual("3.0", p.batt.ToString("F1"));

            var twe = p.convertToTwePacket();
            Assert.AreNotEqual(null, twe);
            Assert.AreEqual(0x8100763A, twe.mac);
            Assert.AreEqual(0x99, twe.lqi);
            Assert.AreEqual(1, twe.btn);
            Assert.AreEqual("3000.0", twe.batt.ToString("F1"));

        }
        [TestMethod]
        public void TestMethod2()
        {
            var p = new TweShotCT();
            int i = 0;
            var msg = ":FECC068100763A0000000099000953485431000500221E";//桁が少ない
            i = searchCollon(msg, i);
            var ans = p.parse(msg, ref i);
            Assert.AreEqual(false, ans);
        }
        [TestMethod]
        public void TestMethod3()
        {
            var p = new TweShotCT();
            int i = 0;
            var msg = ":FECC068100763A0000000099000953485431000500221EF0";//LRC誤り
            i = searchCollon(msg, i);
            var ans = p.parse(msg, ref i);
            Assert.AreEqual(false, ans);
        }

        public static int searchCollon(string msg, int ofs)
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

    }
}
