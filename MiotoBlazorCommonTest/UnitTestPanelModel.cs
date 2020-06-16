using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
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
        public void TestPanelModelNormal()
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
        public void TestPanelModelNormal2()
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
        public void TestPanelModelNoCount()
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
        public void TestPanelModelNoFactor()
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

            Assert.AreEqual(0, panel.bekidou);

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
        public void TestPanelModelNoFactor2()
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

            ct = dummy.flip(mt);//turn off
            panel.updateCycleTime(ct);
            Assert.AreEqual(1, panel.dekidaka);
            Assert.AreEqual(1, panel.signalNum);
            Assert.AreEqual(2, panel.listProductionFactor.Count);
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
            Assert.AreEqual($"{(mt + mt).ToString("F1")}秒", panel.getRunSecStr());
            Assert.AreEqual($"{(ht).ToString("F1")}秒", panel.getStopSecStr());

            Assert.AreEqual(0, panel.bekidou);

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

            orgDt = new DateTime(ct.dt.Ticks);
            while (orgDt.Hour == ct.dt.Hour)
            {
                ct = dummy.flip(ht);//turn on 
                panel.updateCycleTime(ct);
                ct = dummy.flip(mt);//turn off
                panel.updateCycleTime(ct);
            }
            Assert.AreEqual(4, panel.listProductionFactor.Count);
        }


        /*
         * 確認が必要な内容
         * 
         * CT無指定の後に休憩、さらにCT無指定で開始したい場合の処理
         * 　>>　UI側で直前のProductionFactor.endTicksに指定時刻設定するのみ。
         * 　    NOCTの要因は登録しない。
         * 
         * 後付での要因CRUDはどうする
         *  時刻一覧を作り、要因追加、既存の項目を削除、CTやmemoを更新等を行うボタンを設置
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
