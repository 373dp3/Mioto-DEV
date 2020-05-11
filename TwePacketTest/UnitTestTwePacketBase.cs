using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoServer;
using MiotoServer.Query;

namespace TwePacketTest
{
    [TestClass]
    public class UnitTestTwePacketBase
    {
        [TestMethod]
        public void TestTwe25252APacketNormal()
        {
            const string p1 = ":78811501A281021369000120000C4000080F3E3E43622685";
            int ofs = 0;
            ofs = searchCollon(p1, ofs);

            var twe = new Twe2525APacket();

            Assert.AreEqual(true, twe.parse(p1, ref ofs));
            Assert.AreEqual(0xA2, twe.lqi);
            Assert.AreEqual(0x81021369, twe.mac);
            Assert.AreEqual(((float)0x0C40) / 1000f, twe.batt);

        }


        [TestMethod]
        public void TestOk2()
        {
            const string p1 = ":78811501A281021369000120000C4000080F3E3E43622685";//TWE-2525パケット
            var wrapper = MiotoServerWrapper.getInstance();
            var parser = new TwePacketParser();
            parser.parse(p1);

            var db = DbWrapper.getInstance();
            var param = new Param("http://localhost/t2525");
            ParamTypeVolume filter = new ParamTypeVolume();
            filter.update(param);
            Assert.AreEqual(param.type, Param.TYPE.TWE2525);

            var ans = db.getCsv(param);
            if (ans.Contains("81021369") == false)
            {
                Assert.Fail();
            }
        }

        #region tool
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
        #endregion


    }
}
