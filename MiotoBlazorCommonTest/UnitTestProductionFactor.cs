using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoBlazorCommon.Struct;

namespace MiotoBlazorCommonTest
{
    [TestClass]
    public class UnitTestProductionFactor
    {
        [TestMethod]
        public void TestDeserialize()
        {
            var p1 = new ProductionFactor()
            {
                mac = 1,
                id = 1,
                stTicks = DateTime.Now.Ticks,
                status = ProductionFactor.Status.START_PRODUCTION,
                isValid = ProductionFactor.Validation.VALID,
                ct = 15
            };

            var p2 = new ProductionFactor();
            p2.ParseInto("f3,1,8100763a,8d846838ac1bad0,0.0,290,");
            Assert.AreEqual(0x8100763a, p2.mac);
            Assert.AreEqual(0x8d846838ac1bad0, p2.stTicks);
            Assert.AreEqual((ProductionFactor.Status)290, p2.status);

            //通常の製造指示番号を解釈できる事
            p2.ParseInto("f3,1,8100763a,8d846838ac1bad0,0.0,290,test");
            Assert.AreEqual("test", p2.memoJson.itemNumber);

            //製造指示番号に「,」が含まれていても処理できる事
            p2.ParseInto("f3,1,8100763a,8d846838ac1bad0,0.0,290,test,test");
            Assert.AreEqual("test,test", p2.memoJson.itemNumber);

            //MEMO部分がJSONの場合、ProductionFactorMemoJsonとして処理する事
            p2.ParseInto("f3,1,8100763a,8d846838ac1bad0,0.0,290,{\"itemNumber\":\"memo\", \"operatorName\":\"name\"}");
            Assert.AreEqual("memo", p2.memoJson.itemNumber);
            Assert.AreEqual("name", p2.memoJson.operatorName);
        }

        [TestMethod]
        public void TestSerialize()
        {
            var p = new ProductionFactor();
            p.ParseInto("f3,1,8100763a,8d846838ac1bad0,0.0,290,{\"itemNumber\":\"memo\", \"operatorName\":\"name\"}");
            p.memoJson.itemNumber = "memo2";
            Assert.AreEqual("f3,1,8100763a,8d846838ac1bad0,0.0,290,{\"itemNumber\":\"memo2\",\"operatorName\":\"name\"}", p.ToCSV());

        }
    }
}
