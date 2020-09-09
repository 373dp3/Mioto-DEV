using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoBlazorCommon.Struct;

namespace MiotoBlazorCommonTest
{
    [TestClass]
    public class UnitProductionConditions
    {
        [TestMethod]
        public void TestUnitProductionConditions()
        {
            try
            {
                var c = new ProductionConditions("no" + '\t' + "3" + '\t' + "2");
                Assert.AreEqual("no", c.itemNumber);
                Assert.AreEqual(3, c.standardCt);
                Assert.AreEqual(2, c.itemsPerOperation);
                Assert.AreEqual("no" + '\t' + "3" + '\t' + "2", c.ToTSV());

                var d = new ProductionConditions();
                d.Copy(c);
                Assert.AreEqual("no", c.itemNumber);
                Assert.AreEqual(3, c.standardCt);
                Assert.AreEqual(2, c.itemsPerOperation);

            }
            catch (Exception e)
            {
                Assert.Fail();
            }

            try
            {
                var c = new ProductionConditions("no" + '\t' + "3.1" + '\t' + "2");
                Assert.AreEqual("no", c.itemNumber);
                Assert.AreEqual(3.1, c.standardCt);
                Assert.AreEqual(2, c.itemsPerOperation);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

            try
            {
                var c = new ProductionConditions("no" + '\t' + "a" + '\t' + "2");
                Assert.Fail();
            }
            catch (Exception e)
            {
            }

            try
            {
                //取数を省略した時に1としていること
                var c = new ProductionConditions("no" + '\t' + "3");
                Assert.AreEqual("no", c.itemNumber);
                Assert.AreEqual(3, c.standardCt);
                Assert.AreEqual(1, c.itemsPerOperation);
                Assert.AreEqual("no" + '\t' + "3" + '\t' + "1", c.ToTSV());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

            try
            {
                //取数を省略した時に1としていること。末尾にタブがあってもエラーとしてないこと
                var c = new ProductionConditions("no" + '\t' + "3" + '\t');
                Assert.AreEqual("no", c.itemNumber);
                Assert.AreEqual(3, c.standardCt);
                Assert.AreEqual(1, c.itemsPerOperation);
                Assert.AreEqual("no" + '\t' + "3" + '\t' + "1", c.ToTSV());
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

        }

        [TestMethod]
        public void TestUnitConfigTwe()
        {
            {
                var config = new ConfigTwe();
                config.listConditions.Add(new ProductionConditions("no" + '\t' + "3.1" + '\t' + "2"));
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "", config.getConditionsTsv());

                config.listConditions.Add(new ProductionConditions("no2" + '\t' + "3.2" + '\t' + "3"));
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "\r\n"+ "no2" + '\t' + "3.2" + '\t' + "3", config.getConditionsTsv());
            }
            {
                var config = new ConfigTwe();
                //1行のみでも可能であること
                config.setConditionsTsv("no" + '\t' + "3.1" + '\t' + "2" + "");
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "", config.getConditionsTsv());

                //改行コードが\r\nでも可能であること
                config.setConditionsTsv("no" + '\t' + "3.1" + '\t' + "2" + "\r\n" + "no2" + '\t' + "3.2" + '\t' + "3");
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "\r\n" + "no2" + '\t' + "3.2" + '\t' + "3", config.getConditionsTsv());

                //改行コードが\rでも可能であること
                config.setConditionsTsv("no" + '\t' + "3.1" + '\t' + "2" + "\r" + "no2" + '\t' + "3.2" + '\t' + "3");
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "\r\n" + "no2" + '\t' + "3.2" + '\t' + "3", config.getConditionsTsv());

                //改行コードが\nでも可能であること
                config.setConditionsTsv("no" + '\t' + "3.1" + '\t' + "2" + "\n" + "no2" + '\t' + "3.2" + '\t' + "3");
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "\r\n" + "no2" + '\t' + "3.2" + '\t' + "3", config.getConditionsTsv());

                //解釈不能なデータが入っても無視をし、Exceptionを挙げないこと
                config.setConditionsTsv("no" + '\t' + "3.1" + '\t' + "2" + "\na,a,a\n" + "no2" + '\t' + "3.2" + '\t' + "3");
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "\r\n" + "no2" + '\t' + "3.2" + '\t' + "3", config.getConditionsTsv());
            }
        }

        [TestMethod]
        public void TestUnitConfigTweSort()
        {
            {
                var config = new ConfigTwe();
                config.listConditions.Add(new ProductionConditions("no" + '\t' + "3.1" + '\t' + "2"));
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "", config.getConditionsTsv());

                config.listConditions.Add(new ProductionConditions("no2" + '\t' + "3.2" + '\t' + "3"));
                Assert.AreEqual("no" + '\t' + "3.1" + '\t' + "2" + "\r\n" + "no2" + '\t' + "3.2" + '\t' + "3", config.getConditionsTsv());
                var a = config.listConditions[0];

                config.moveConditions(a, false);
                Assert.AreEqual(
                    "no2" + '\t' + "3.2" + '\t' + "3"
                    + "\r\n" +
                    "no" + '\t' + "3.1" + '\t' + "2"
                    , config.getConditionsTsv());

                config.moveConditions(a, false);
                Assert.AreEqual(
                    "no2" + '\t' + "3.2" + '\t' + "3"
                    + "\r\n" +
                    "no" + '\t' + "3.1" + '\t' + "2"
                    , config.getConditionsTsv());

                config.moveConditions(a, true);
                Assert.AreEqual(
                    "no" + '\t' + "3.1" + '\t' + "2"
                    + "\r\n" +
                    "no2" + '\t' + "3.2" + '\t' + "3"
                    , config.getConditionsTsv());

                config.moveConditions(a, true);
                Assert.AreEqual(
                    "no" + '\t' + "3.1" + '\t' + "2"
                    + "\r\n" +
                    "no2" + '\t' + "3.2" + '\t' + "3"
                    , config.getConditionsTsv());
            }
        }
    }
}
