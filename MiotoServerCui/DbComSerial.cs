using MiotoServer.DB;
using MiotoServer.Query;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class DbComSerial
    {
        public static DbComSerial instance { get; private set; } = null;

        public static DbComSerial getInstance()
        {
            if (instance == null) { instance = new DbComSerial(); }
            return instance;
        }

        SQLite.SQLiteConnection conn;

        private DbComSerial()
        {
            conn = new SQLiteConnection(":memory:");
            conn.CreateTable<SerialCache>();
            conn.CreateTable<Mac2Ampere>();
        }
        private void d(string msg) { MiotoServerWrapper.d(msg); }

        public void insert(TweComSerialPacket packet)
        {
            string query = "INSERT INTO SerialCache (mac, key, ticks, csv) "
                + $" values ({packet.mac}, '{packet.key}', {packet.dt.Ticks}, '{packet.csv}')";            
            conn.Execute(query);

            if (packet.values.Count > 0)
            {
                var val = conn.Table<Mac2Ampere>().Where(v => v.mac == packet.mac).FirstOrDefault();
                if (val == null)
                {
                    val = new Mac2Ampere();
                    val.mac = packet.mac;
                    val.ticks = packet.dt.Ticks;
                    conn.Insert(val);
                }
                else
                {
                    //時刻またぎ判定
                    if ((new DateTime(val.ticks)).Hour != packet.dt.Hour)
                    {
                        //DBに登録
                        DbWrapper.getInstance().insertCsv(val);
                        //クリア
                        Mac2Ampere.clear(val);
                    }
                    Mac2Ampere.update(val, packet);
                    conn.Update(val);
                }
            }
        }

        public List<string> getMac2AmpereList()
        {
            var ans = new List<string>();

            foreach(var item in conn.Table<Mac2Ampere>())
            {
                ans.Add(Mac2Ampere.ToCSV(item));
            }
            return ans;
        }

        public string getCsv(Param param)
        {
            StringBuilder sb = new StringBuilder();
            if (param.memDbKey.CompareTo("ct10") == 0)
            {
                sb.Append("datetime,mac,ch1,ch2,ch3,ch4,ch5" + Environment.NewLine);
            }

            // finalの指示がある時だけSQL、それ以外はLINQで対応する。
            if(param.volume == Param.VOLUME.FINAL)
            {
                var macListStr = "";
                if (param.macList.Count > 0)
                {
                    macListStr = " and ( mac = " + string.Join(" or mac = ", param.macList) + " ) ";
                }
                var queryStr = "select mac, key, max(ticks) as ticks, csv from SerialCache " +
                    $" where key=\"{param.memDbKey}\" {macListStr}"+
                    " group by mac";
                var rs = conn.Query<SerialCache>(queryStr);
                foreach(var item in rs)
                {
                    var dt = new DateTime(Convert.ToInt64(item.ticks)).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    sb.Append($"{dt},{item.mac.ToString("X")},{item.csv}" + Environment.NewLine);
                }
                return sb.ToString();
            }


            TableQuery<SerialCache> query = null;
            int limitRow = int.MaxValue;
            if(param.fixRow > 0) { limitRow = (int)param.fixRow; }
            if((param.macList!=null) && (param.macList.Count > 0))
            {
                query = conn.Table<SerialCache>()
                        .Where(q => param.macList.Contains(q.mac))
                        .Where(q => q.key.Equals(param.memDbKey))
                        .OrderByDescending(q => q.ticks)
                        .Take(limitRow);
            }
            else
            {
                //単独
                query = conn.Table<SerialCache>()
                        .Where(q => q.key.Equals(param.memDbKey))
                        .OrderByDescending(q => q.ticks)
                        .Take(limitRow);
            }

            int cnt = 0;
            foreach(var cache in query)
            {
                var dt = new DateTime(Convert.ToInt64(cache.ticks)).ToString("yyyy/MM/dd HH:mm:ss.fff");
                sb.Append($"{dt},{cache.mac.ToString("X")},{cache.csv}" + Environment.NewLine);
                cnt++;
            }
            for (int i = cnt; i < param.fixRow; i++)
            {
                sb.Append(Environment.NewLine);
            }

            if (param.fixRow > 0) { sb.Append(" "); }

            return sb.ToString();

        }

        public void purgeBySec(long sec)
        {
            var ticks = DateTime.Now.AddSeconds(-1 * sec).Ticks;

            //削除
            string query = $"delete from SerialCache where ticks < {ticks}";
            conn.Execute(query);
        }

    }
}
