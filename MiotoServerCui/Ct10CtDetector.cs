using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MiotoServer.CfgOption.SerialCurrent;

namespace MiotoServer
{
    public class Ct10CtDetector
    {
        public static Ct10CtDetector instance { get; private set; }
        public static Ct10CtDetector getInstance()
        {
            if(instance == null) { instance = new Ct10CtDetector(); }
            return instance;
        }
        private Ct10CtDetector() { }

        private  void d(string msg)
        {
            MiotoServerWrapper.d(msg);
        }

        public void fetch(TweComSerialPacket packet)
        {
            try
            {
                var cfg = MiotoServerWrapper.config.listSerialCurrent
                    .Where(v => v.mac == packet.mac)
                    .First()
                    .listChInfo;
                int ch = 0;
                foreach(var amp in packet.values)
                {
                    //LtoHしきい値を超え、High配列に含まれなければ登録、Low配列、Low暫定配列から削除
                    //HtoLしきい値を下回り、Low配列に含まれなければ暫定登録
                    //HtoLしきい値を下回り、Low配列の暫定期間を超えた場合は正式登録、High配列から削除
                    //HtoLしきい値を上回る場合、Low配列中の暫定登録があれば削除                    

                    var c = cfg[ch];
                    //LtoHしきい値を超え、High配列に含まれなければ登録、Low配列、Low暫定配列から削除
                    if (amp > c.thresholdLtoH)
                    {
                        //High登録
                        if(insertIfNotExist(listHigh, packet, ch, DetectInfo.Condition.High))
                        {
                            d("High登録" + ch);
                            updateCtInfo(packet, ch, true, c);
                        }

                        //Low配列、Low暫定配列から削除
                        delete(listLow, packet, ch, DetectInfo.Condition.HtoL_Temp);
                        delete(listLow, packet, ch, DetectInfo.Condition.Low);
                    }
                    if (amp < c.thresholdHtoL)
                    {
                        //HtoLしきい値を下回り、Low配列に含まれなければ暫定登録
                        var aryLorLTemp = listLow.Where(v => (v.mac == packet.mac) && (v.ch == ch));
                        if (aryLorLTemp.Count() == 0)
                        {
                            insertIfNotExist(listLow, packet, ch, DetectInfo.Condition.HtoL_Temp);
                            d("Low暫定登録" + ch);
                        }
                        //HtoLしきい値を下回り、Low配列の暫定期間を超えた場合は正式登録、High配列から削除
                        var ary = getArray(listLow, packet, ch, DetectInfo.Condition.HtoL_Temp);
                        foreach(var item in ary)
                        {
                            var span = DateTime.Now - item.dt;
                            if(span.TotalMilliseconds < c.avoidStopDetectionMsec) { continue; }
                            //正式登録へ変更
                            item.cnd = DetectInfo.Condition.Low;
                            d("Low正式登録" + ch);
                            updateCtInfo(packet, ch, false, c);
                            //High配列削除
                            delete(listHigh, packet, ch, DetectInfo.Condition.High);
                        }
                    }

                    //
                    ch++;
                }
                
            }
            catch (Exception e)
            {
                d(e.ToString());
            }
        }
        private void updateCtInfo(TweComSerialPacket packet, int ch, bool isHigh, ChInfo c)
        {
            int btn = 0;
            UInt32 mac = packet.mac;
            mac = (mac << 4) + (uint)ch + 1;
            if (isHigh) { btn = 1; }

            var dt = new DateTime(packet.dt.Ticks);
            if (isHigh == false)
            {
                dt = dt.AddMilliseconds(-1*c.avoidStopDetectionMsec);
            }
            var twe = new TweCtPacket(mac, packet.seq, (byte)btn, 255, 3, dt.Ticks);
            TwePacketParser.buildAndRegistCtInfo(1, twe);
        }

        private List<DetectInfo> listHigh = new List<DetectInfo>();
        private List<DetectInfo> listLow = new List<DetectInfo>();

        /// <summary>
        /// 配列に存在しない場合、登録する。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="packet"></param>
        /// <param name="ch"></param>
        /// <param name="cnd"></param>
        /// <returns>登録した場合にtrue, すでに存在した場合はfalse</returns>
        private bool insertIfNotExist(List<DetectInfo> list, TweComSerialPacket packet, int ch, DetectInfo.Condition cnd)
        {
            var ary = getArray(list, packet, ch, cnd);
            if (ary.Count() == 0)
            {
                list.Add(new DetectInfo
                {
                    mac = packet.mac,
                    ch = ch,
                    cnd = cnd,
                    dt = DateTime.Now
                });
                return true;
            }
            return false;
        }
        private void delete(List<DetectInfo> list, TweComSerialPacket packet, int ch, DetectInfo.Condition cnd)
        {
            list.RemoveAll(v => (v.mac == packet.mac) && (v.ch == ch) && (v.cnd == cnd));
        }

        private static IEnumerable<DetectInfo> getArray(List<DetectInfo> list, TweComSerialPacket packet, int ch, DetectInfo.Condition cnd)
        {
            return list.Where(v => (v.mac == packet.mac) && (v.ch == ch) && (v.cnd == cnd));
        }

        public class DetectInfo
        {
            public long mac { get; set; }
            public int ch { get; set; }
            public Condition cnd { get; set; }
            public DateTime dt { get; set; }
            public enum Condition
            {
                High, HtoL_Temp, Low
            }
        }
    }
}
