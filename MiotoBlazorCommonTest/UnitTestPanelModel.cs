using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiotoBlazorCommon;
using MiotoBlazorCommon.Struct;

namespace MiotoBlazorCommonTest
{
    [TestClass]
    public class UnitTestPanelModel
    {
        /// <summary>
        /// 時間丁度に稼働始点信号が入る場合
        /// </summary>
        [TestMethod]
        public void TestNormal()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2019, 10, 28, 9, 0, 0);
            panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1, id = 1, stTicks = dt.Ticks,
                    status = ProductionFactor.Status.START_PRODUCTION,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });

            var dummy = new DummyCt(dt.AddSeconds(-15), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00丁度のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(2, panel.dekidaka);
            Assert.AreEqual(2, panel.signalNum);

            Assert.AreEqual((2 * (mt + ht))/(2 * mt + ht), panel.bekidou);
        }

        /// <summary>
        /// 時間丁度に稼働始点信号が間に合わない場合
        /// </summary>
        [TestMethod]
        public void TestNormal2()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2019, 10, 28, 9, 0, 0);
            panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1,
                    id = 1,
                    stTicks = dt.Ticks,
                    status = ProductionFactor.Status.START_PRODUCTION,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });

            var dummy = new DummyCt(dt.AddSeconds(-16), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00 マイナス1秒のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);//稼働としてカウントしないが信号はカウント

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(2, panel.signalNum);

            Assert.AreEqual(((mt + ht)) / ((mt-1) + ht + mt), panel.bekidou);
        }

        /// <summary>
        /// START_PRODUCTION以外は出来高にカウントしない
        /// </summary>
        [TestMethod]
        public void TestNoCount()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2019, 10, 28, 9, 0, 0);
            panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1,
                    id = 1,
                    stTicks = dt.Ticks,
                    status = ProductionFactor.Status.START_PRODUCTION,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });

            var dummy = new DummyCt(dt.AddSeconds(-16), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00 マイナス1秒のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);//稼働としてカウントしないが信号はカウント

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(2, panel.signalNum);

            Assert.AreEqual(((mt + ht)) / ((mt - 1) + ht + mt), panel.bekidou);

            var list = new List<ProductionFactor.Status>() {
                ProductionFactor.Status.FINISH_CHANGE_PRODUCTION,
                ProductionFactor.Status.FINISH_REPAIRING,
                ProductionFactor.Status.NOOP,
                ProductionFactor.Status.START_BRAKEDOWN_STOP,
                ProductionFactor.Status.START_CHANGE_PRODUCTION,
                ProductionFactor.Status.START_PLANNED_STOP,
                ProductionFactor.Status.START_REPAIRING,
                ProductionFactor.Status.START_REST,
                ProductionFactor.Status.WAITING_FOR_PARTS,
            };
            foreach(var s in list)
            {
                var preFactor = panel.listProductionFactor[panel.listProductionFactor.Count - 1];
                panel.SetProductionFactor(
                        new ProductionFactor()
                        {
                            mac = 1,
                            id = preFactor.id + 1,
                            stTicks = ct.dt.Ticks,
                            status = s,
                            isValid = ProductionFactor.Validation.VALID,
                        });
                for (var i = 0; i < 3; i++)
                {
                    ct = dummy.flip(ht);//turn on 
                    panel.updateCycleTime(ct);
                    Assert.AreEqual(1, panel.dekidaka);

                    ct = dummy.flip(mt);//turn off
                    panel.updateCycleTime(ct);
                    Assert.AreEqual(1, panel.dekidaka);
                }
            }
        }

        /// <summary>
        /// 生産要因が未登録の場合
        /// </summary>
        [TestMethod]
        public void TestNoFactor()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2019, 10, 28, 9, 0, 0);
            /* panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1,
                    id = 1,
                    stTicks = dt.Ticks,
                    status = ProductionFactor.Status.START_PRODUCTION,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });//*/

            var dummy = new DummyCt(dt.AddSeconds(-15), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00丁度のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);
            Assert.AreEqual(1, panel.listProductionFactor.Count);
            Assert.AreEqual($"{(mt).ToString("F1")}秒", panel.getRunSecStr());
            Assert.AreEqual($"{(0.0).ToString("F1")}秒", panel.getStopSecStr());

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);
            Assert.AreEqual($"{(mt).ToString("F1")}秒", panel.getRunSecStr());
            Assert.AreEqual($"{(ht).ToString("F1")}秒", panel.getStopSecStr());

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(2, panel.dekidaka);
            Assert.AreEqual(2, panel.signalNum);
            Assert.AreEqual($"{(mt+mt).ToString("F1")}秒", panel.getRunSecStr());
            Assert.AreEqual($"{(ht).ToString("F1")}秒", panel.getStopSecStr());

            Assert.AreEqual((2*mt)/(2*mt+ht), panel.bekidou);

            //1時間超えまでループ
            var orgDt = new DateTime(ct.dt.Ticks);
            while(orgDt.Hour == ct.dt.Hour)
            {
                ct = dummy.flip(ht);//turn on 
                panel.updateCycleTime(ct);
                ct = dummy.flip(mt);//turn off
                panel.updateCycleTime(ct);
            }
            Assert.AreEqual(2, panel.listProductionFactor.Count);

            orgDt = new DateTime(ct.dt.Ticks);
            while (orgDt.Hour == ct.dt.Hour)
            {
                ct = dummy.flip(ht);//turn on 
                panel.updateCycleTime(ct);
                ct = dummy.flip(mt);//turn off
                panel.updateCycleTime(ct);
            }
            Assert.AreEqual(3, panel.listProductionFactor.Count);
        }

        /// <summary>
        /// 生産要因は登録されているが未登録時間帯の信号を受信した場合
        /// </summary>
        [TestMethod]
        public void TestNoFactor2()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2019, 10, 28, 9, 0, 0);
            //*
            panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1,
                    id = 1,
                    stTicks = dt.AddHours(3).Ticks,
                    status = ProductionFactor.Status.START_PRODUCTION,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });//*/

            var dummy = new DummyCt(dt.AddSeconds(-15), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00丁度のTurn on

            /*
             * 注意点
             * 
             * CT無指定は1時間毎のLazyな処理だが、長時間(3時間等)操作が無い後に
             * 計画停止等の要因が入力された後に「あんどん」等へページ遷移を行った場合、
             * CT無指定の要因が、期待する1時間毎の枠ではなく、3時間分の枠になってしまう。
             * endTicksを設定する時点で確認が必要か。
             * 　V
             * NOCTの場合、NOCT側のendTicksが短くなる場合にのみendTicksを上書きする。
             * NOCT以外でもlong.MaxValueより実際のTicksのほうがDurationは短くなるので、
             * NOCTの確認処理を加えなくても同様になる。
             * 
             * NOCTの処理において、1時間を超える信号は破棄するようになっているため、
             * 生産要因の抜けは許容する。
             * 
             */
            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);
            Assert.AreEqual(2, panel.listProductionFactor.Count);
            Assert.AreEqual($"{(mt).ToString("F1")}秒", panel.getRunSecStr());
            Assert.AreEqual("59.8分", panel.getStopSecStr());

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(2, panel.dekidaka);
            Assert.AreEqual(2, panel.signalNum);

            Assert.AreEqual((2*mt)/(3600), panel.bekidou);

            //1時間超えまでループ
            var orgDt = new DateTime(ct.dt.Ticks);
            while (orgDt.Hour == ct.dt.Hour)
            {
                ct = dummy.flip(ht);//turn on 
                panel.updateCycleTime(ct);
                ct = dummy.flip(mt);//turn off
                panel.updateCycleTime(ct);
            }
            Assert.AreEqual(3, panel.listProductionFactor.Count);
            for(var i=0; i<panel.listProductionFactor.Count-1; i++)
            {
                var f = panel.listProductionFactor[i];
                Assert.AreEqual(true, f.isFixed);
                Assert.AreEqual(ProductionFactor.Status.START_PRODUCTION_NOCT, f.status);
            }

            orgDt = new DateTime(ct.dt.Ticks);
            while (orgDt.Hour == ct.dt.Hour)
            {
                ct = dummy.flip(ht);//turn on 
                panel.updateCycleTime(ct);
                ct = dummy.flip(mt);//turn off
                panel.updateCycleTime(ct);
            }
            Assert.AreEqual(4, panel.listProductionFactor.Count);
            for (var i = 0; i < panel.listProductionFactor.Count - 1; i++)
            {
                var f = panel.listProductionFactor[i];
                Assert.AreEqual(true, f.isFixed);
                Assert.AreEqual(ProductionFactor.Status.START_PRODUCTION_NOCT, f.status);
            }
        }

        /// <summary>
        /// CT無指定において長大なOFF信号を受けた場合(終業から翌朝まで消灯等)、信号長が集計枠である1時間を超えるものは破棄する。
        /// </summary>
        [TestMethod]
        public void TestLongOff()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2020, 6, 16, 16, 47, 0);
            

            var dummy = new DummyCt(dt, false);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(ht);//turn on
            ct = dummy.flip(mt);//turn off

            ct = dummy.flip(64430.8);//turn on 17時間超
            panel.updateCycleTime(ct);
            Assert.AreEqual(0, panel.listProductionFactor.Count);//新規要因を追加していないこと。
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(0, panel.signalNum);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.listProductionFactor.Count);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);

            ct = dummy.flip(ht);//turn on
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);
        }
        /// <summary>
        /// CT無指定において長大なON信号を受けた場合(終業から翌朝まで消灯等)、信号長が集計枠である1時間を超えるものは破棄する。
        /// </summary>
        [TestMethod]
        public void TestLongOn()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = new DateTime(2020, 6, 16, 16, 47, 0);


            var dummy = new DummyCt(dt, true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on

            ct = dummy.flip(64430.8);//turn off 17時間超
            panel.updateCycleTime(ct);
            Assert.AreEqual(0, panel.listProductionFactor.Count);//新規要因を追加していないこと。
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(0, panel.signalNum);

            ct = dummy.flip(ht);//turn on
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.listProductionFactor.Count);
            Assert.AreEqual(0, panel.dekidaka);
            Assert.AreEqual(0, panel.signalNum);

            ct = dummy.flip(ht);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.listProductionFactor.Count);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);
        }


        /// <summary>
        /// あんどん表示の停止時間・稼働時間における進み方の確認
        /// (現在時刻に依存するテストのため、時間マタギでエラーになることもある)
        /// </summary>
        [TestMethod]
        public void TestSummaryTicksCheck()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = DateTime.Now;
            var dummy = new DummyCt(dt.AddSeconds(-25), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00丁度のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.listProductionFactor.Count);
            var span = new TimeSpan(ct.dt.Ticks - panel.listProductionFactor[0].stTicks);
            //信号の原点（受信時刻-CT10）がパネルの開始時刻になっていること
            Assert.AreEqual(mt, span.TotalSeconds);
            if (span.TotalSeconds - (ct.ct10)> 1.5)
            {
                Assert.Fail();
            }
            Assert.AreEqual(0, (int)panel.getStopSec());
            Assert.AreEqual(10, (int)panel.getRunSec());

            Thread.Sleep(1000);//1秒経過した時に、停止側のみが経過しているか
            Assert.AreEqual(1, (int)panel.getStopSec());
            Assert.AreEqual(10, (int)panel.getRunSec());

            ct = dummy.flip( 1 /*ht*/);//turn on 
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, (int)(panel.getStopSec() * 10 + 0.5) / 10);
            Assert.AreEqual(10, (int)(panel.getRunSec() * 10 + 0.5) / 10);

            Thread.Sleep(1000);//1秒経過した時に、稼働側のみが経過しているか
            var pre = panel.getStopSec();
            var preRun = panel.getRunSec();
            var preDurTicks = DateTime.Now.Ticks - panel.listProductionFactor[0].stTicks;
            Assert.AreEqual(1, (int)(panel.getStopSec() * 10 + 0.5) / 10);
            Assert.AreEqual(11, (int)(panel.getRunSec() * 10 + 0.5) / 10);

            //停止側のみ僅かに進んでしまう問題の確認
            Thread.Sleep(1000);//1秒経過した時に、稼働側のみが経過しているか
            var curDurTicks = DateTime.Now.Ticks - panel.listProductionFactor[0].stTicks;
            //Console.WriteLine($"{preRun} {panel.getRunSec()} {preDurTicks} {curDurTicks}");
            var diff = panel.getStopSec() - pre;
            if(diff>0.0001) { Assert.Fail(); }
            Assert.AreEqual(12, (int)(panel.getRunSec() * 10 + 0.5) / 10);
        }

        /// <summary>
        /// CT無し生産の後に、それ以外の要因が入った場合、無し生産の終了時刻が次の要因の登録時刻になること
        /// </summary>
        [TestMethod]
        public void TestPlanedStopAfterNoCt()
        {
            var panel = new PanelModel() { mac = 1, };
            var dt = DateTime.Now;
            /* panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1,
                    id = 1,
                    stTicks = dt.Ticks,
                    status = ProductionFactor.Status.START_PRODUCTION,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });//*/

            var dummy = new DummyCt(dt.AddSeconds(-40), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00丁度のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);

            //計画停止の登録
            panel.SetProductionFactor(
                    new ProductionFactor()
                    {
                        mac = 1,
                        id = 1,
                        stTicks = ct.dt.AddSeconds(1).Ticks,
                        status = ProductionFactor.Status.START_PLANNED_STOP,
                        isValid = ProductionFactor.Validation.VALID,
                    });
            Assert.AreEqual(2, panel.listProductionFactor.Count);
            Assert.AreEqual(panel.listProductionFactor[0].endTicks, ct.dt.AddSeconds(1).Ticks);

            var duration = panel.listProductionFactor[0].GetDurationSec();
            Thread.Sleep(1000);

            var factor = panel.listProductionFactor[0];
            Assert.AreNotEqual(long.MaxValue, factor.endTicks);
            Assert.AreNotEqual(0, factor.lastCycleTicks);
            Assert.AreEqual(Round01(duration), Round01(factor.GetDurationSec()));
            Assert.AreEqual(true, panel.listProductionFactor[0].isFixed);


            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);
            var preStop = panel.getStopSec();
            Thread.Sleep(1000);
            Assert.AreEqual(preStop, panel.getStopSec());

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            var preRun = panel.getRunSec();
            Thread.Sleep(1000);
            Assert.AreEqual(preRun, panel.getRunSec());

        }


        /// <summary>
        /// CT無し生産の後に、それ以外の要因が入った場合、無し生産の終了時刻が次の要因の登録時刻になること
        /// (HttpでCSV形式の要因定義を受信した場合)
        /// </summary>
        [TestMethod]
        public void TestPlanedStopAfterNoCtByHttpCsv()
        {
            /*
             * テスト実施時刻が16:01など、1分未満の状態だと、
             * テストに失敗するので注意
             * 
             * */
            var panel = new PanelModel() { mac = 1, };
            var dt = DateTime.Now;
            panel.SetProductionFactor(
                new ProductionFactor()
                {
                    mac = 1,
                    id = 1,
                    stTicks = dt.Ticks,
                    status = ProductionFactor.Status.START_PLANNED_STOP,
                    isValid = ProductionFactor.Validation.VALID,
                    ct = 15
                });//*/

            var dummy = new DummyCt(dt.AddSeconds(-30), true);
            CycleTime ct = null;
            var mt = 10.0;  //マシンタイム
            var ht = 5.0;   //手作業時間(Human time)

            //CT00～CT11を揃えるため2回捨てる
            ct = dummy.flip(mt);//turn off
            ct = dummy.flip(ht);//turn on 
            // 上は9:00丁度のTurn on

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);

            ct = dummy.flip(ht);//turn on 
            panel.updateCycleTime(ct);

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);

            //計画停止の登録(事前に登録済み)
            Assert.AreEqual(2, panel.listProductionFactor.Count);

            var duration = panel.listProductionFactor[0].GetDurationSec();
            Thread.Sleep(1000);

            var factor = panel.listProductionFactor[0];
            Assert.AreNotEqual(long.MaxValue, factor.endTicks);
            Assert.AreEqual(true, panel.listProductionFactor[0].isFixed);
            Assert.AreNotEqual(0, factor.lastCycleTicks);
            Assert.AreEqual(Round01(duration), Round01(factor.GetDurationSec()));
        }


        /// <summary>
        /// 生産以外の要因が登録されているときの所要時間計算
        /// </summary>
        [TestMethod]
        public void TestNoProductionDuration()
        {
            var panel = new PanelModel() { mac = 1, };
            //計画停止の登録
            panel.SetProductionFactor(
                    new ProductionFactor()
                    {
                        mac = 1,
                        id = 1,
                        stTicks = DateTime.Now.Ticks,
                        status = ProductionFactor.Status.START_PLANNED_STOP,
                        isValid = ProductionFactor.Validation.VALID,
                    });
            Thread.Sleep(2000);
            panel.SetProductionFactor(
                    new ProductionFactor()
                    {
                        mac = 1,
                        id = 2,
                        stTicks = DateTime.Now.Ticks,
                        status = ProductionFactor.Status.START_REPAIRING,
                        isValid = ProductionFactor.Validation.VALID,
                    });
            Thread.Sleep(2000);
            //1日の合計稼働時間に影響しないこと
            Assert.AreEqual(0, panel.getStopSec());
            Assert.AreEqual(0, panel.getRunSec());
            
            Assert.AreEqual(2, Round01(panel.listProductionFactor[0].GetDurationSec()));
            //終了未定義かつ最終信号もないため、0（1日の終わりに計画停止を指定した等）
            Assert.AreEqual(0, Round01(panel.listProductionFactor[1].GetDurationSec()));
        }


        public double Round01(double val)
        {
            return Math.Round(val * 10) / 10;
        }


        /*
         * 確認が必要な内容
         * 
         * CT無指定の後に休憩、さらにCT無指定で開始したい場合の処理
         * 　>>　UI側で直前のProductionFactor.endTicksに指定時刻設定するのみ。
         * 　    NOCTの要因は登録しない。
         * 
         * 後付での要因CRUDはどうする
         *  時刻一覧を作り、要因追加、既存の項目を削除、CTやmemoの更新等を行うボタンを設置
         *  更新内容支援クラスを作り、最終的にSaveなら確定させる。
         * 
         */


        [TestMethod]
        public void TestDummyCt()
        {
            var dt = new DateTime(2019, 10, 28, 9, 0, 0);

            var dummy = new DummyCt(dt, true);
            CycleTime ct = null;
            ct = dummy.flip(10);    //turn off
            Assert.AreEqual(10, ct.ct10);
            Assert.AreEqual(0, ct.btn);
            ct = dummy.flip(5);     //turn on
            Assert.AreEqual(5, ct.ct01);
            Assert.AreEqual(15, ct.ct11);
            Assert.AreEqual(1, ct.btn);
            ct = dummy.flip(10);    //turn off
            Assert.AreEqual(10, ct.ct10);
            Assert.AreEqual(15, ct.ct00);
            Assert.AreEqual(0, ct.btn);
            ct = dummy.flip(5);     //turn on
            Assert.AreEqual(5, ct.ct01);
            Assert.AreEqual(15, ct.ct11);
            Assert.AreEqual(1, ct.btn);
        }

        public class DummyCt
        {
            private DateTime dt;
            private bool isOn = false;
            private long mac = 0;
            private double preProgSec = 0;
            public DummyCt(DateTime orgDt, bool isOn, long mac=1)
            {
                this.dt = orgDt;
                this.isOn = isOn;
                this.mac = mac;
            }

            public CycleTime flip(double progSec)
            {
                CycleTime ans = null;
                var currentDt = dt.AddSeconds(progSec);
                isOn = !isOn;
                if (isOn)
                {
                    ans = new CycleTime()
                    {
                        mac = mac,
                        btn = 1,
                        createDt = currentDt,
                        seq = 1,
                        dt = currentDt,
                        ct01 = progSec,
                        ct11 = preProgSec + progSec,
                    };
                }
                else
                {
                    ans = new CycleTime()
                    {
                        mac = mac,
                        btn = 0,
                        createDt = currentDt,
                        seq = 1,
                        dt = currentDt,
                        ct10 = progSec,
                        ct00 = preProgSec + progSec,
                    };

                }

                //
                preProgSec = progSec;
                dt = currentDt;
                //
                return ans;
            }
        }

        private CycleTime GetCycle(DateTime dt, int mtSec, int humTime)
        {
            return new CycleTime()
            {
                mac = 1,
                btn = 1,
                createDt = dt.AddSeconds(mtSec),
                seq = 1,
                dt = dt.AddSeconds(mtSec),
                ct01 = mtSec,
                ct11 = mtSec + humTime,
            };
        }
    }
}
