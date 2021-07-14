using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoServer;
using System;

namespace TwePacketTest
{
    [TestClass]
    public class UnitTestTwePalSensePacket
    {
        [TestMethod]
        public void TestParseAmbientSense()
        {
            {
                var p = new TwePalSensePacket();
                int i = 0;
                var msg = ":80000000990002810C68A701808205113008020AA01130010203D8050100020805010200021AD1020300040000000C8C87";
                i = searchCollon(msg, i);
                var ans = p.parse(msg, ref i);
                Assert.AreEqual(true, ans);
                Assert.AreEqual(0x810C68A7, p.mac);
                Assert.AreEqual(153, p.lqi);
                Assert.AreEqual("2.7", p.batt.ToString("F1"));
                Assert.AreEqual("20.5", p.temp.ToString("F1"));
                Assert.AreEqual("68.7", p.humi.ToString("F1"));
            }
        }

        [TestMethod]
        public void TestMagSense()
        {
            {
                var p = new TwePalSensePacket();
                int i = 0;
                var msg = ":800000009C003E810B7FD901808103113008020BA411300102048300000001803245";
                i = searchCollon(msg, i);
                var ans = p.parse(msg, ref i);
                Assert.AreEqual(true, ans);
                Assert.AreEqual(TwePalSensePacket.CODE.MAGG, p.code);
                Assert.AreEqual(156, p.lqi);
                Assert.AreEqual(0x80, p.magPole);//磁石なしかつ、1分定期ポーリング(最上位ビット1, 0x00 + 0x80)
                Assert.AreEqual("3.0", p.batt.ToString("F1"));
            }

            {
                var p = new TwePalSensePacket();
                int i = 0;
                var msg = ":800000009C0033810B7FD901808103113008020BB811300102053100000001008FB0";
                i = searchCollon(msg, i);
                var ans = p.parse(msg, ref i);
                Assert.AreEqual(true, ans);
                Assert.AreEqual(TwePalSensePacket.CODE.MAGG, p.code);
                Assert.AreEqual(156, p.lqi);
                Assert.AreEqual(0x00, p.magPole);//磁石無し
                Assert.AreEqual("3.0", p.batt.ToString("F1"));
            }

            {
                var p = new TwePalSensePacket();
                int i = 0;
                var msg = ":80000000A80006810B7FD901808103113008020B721130010205570000000102E995";
                i = searchCollon(msg, i);
                var ans = p.parse(msg, ref i);
                Assert.AreEqual(true, ans);
                Assert.AreEqual(TwePalSensePacket.CODE.MAGG, p.code);
                Assert.AreEqual(0x02, p.magPole);//S極
            }
            {
                var p = new TwePalSensePacket();
                int i = 0;
                var msg = ":80000000A80015810B7FD901808103113008020B8611300102052C0000000101EF98";
                i = searchCollon(msg, i);
                var ans = p.parse(msg, ref i);
                Assert.AreEqual(true, ans);
                Assert.AreEqual(TwePalSensePacket.CODE.MAGG, p.code);
                Assert.AreEqual(0x01, p.magPole);//N極
            }
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
