using MiotoServer.Query;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
            if(instance == null)
            {
                instance = new DbComSerial();
            }
            return instance;
        }

        SQLiteConnection conn = null;

        private DbComSerial()
        {
            var sqlConnectionSb = new SQLiteConnectionStringBuilder
            {
                DataSource = ":memory:"
            };
            conn = new SQLiteConnection(sqlConnectionSb.ToString());
            conn.Open();

            using (var tran = conn.BeginTransaction())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE [cache] ([mac] INTEGER,[key] TEXT, [ticks] INTEGER,[csv] TEXT) ";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
        }

        private void d(string msg)
        {
            Program.d(msg);
        }

        DbWrapper dbWrapper = DbWrapper.getInstance();

        public void insert(TweComSerialPacket packet)
        {
            string query = "INSERT INTO cache (mac, key, ticks, csv) " 
                + $" values ({packet.mac}, '{packet.key}', {DateTime.Now.Ticks}, '{packet.csv}')";

            dbWrapper.updateDateFlg();//ブラウザ取得時のNot modified対策

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
        }

        public string getCsv(Param param)
        {
            StringBuilder sb = new StringBuilder();
            if (param.memDbKey.CompareTo("ct10") == 0)
            {
                sb.Append("datetime,mac,ch1,ch2,ch3,ch4,ch5" + Environment.NewLine);
            }
            long cnt = 0;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = $"select mac, ticks, csv from cache where key='{param.memDbKey}' ";
                if (param.macList.Count > 0)
                {
                    var macQuery = " (mac='" + String.Join("' or mac='", param.macList) + "') ";
                    readCmd.CommandText += " and " + macQuery;
                }
                readCmd.CommandText += "order by ticks DESC";
                if (param.fixRow > 0)
                {
                    readCmd.CommandText += $" limit {param.fixRow} ";
                }
                using (var reader = readCmd.ExecuteReader())
                {
                    for (var i = 0; reader.Read(); i++)
                    {
                        cnt++;
                        if (reader[0].ToString().Length == 0) continue;
                        var dt = new DateTime(Convert.ToInt64(reader[1])).ToString("yyyy/MM/dd HH:mm:ss.fff");
                        var mac = Convert.ToUInt64(reader[0].ToString());
                        sb.Append($"{dt},{mac.ToString("X")},{reader[2].ToString()}" + Environment.NewLine);
                    }
                }
            }
            for(long i=cnt; i< param.fixRow; i++)
            {
                sb.Append(Environment.NewLine);
            }
            if(param.fixRow>0) { sb.Append(" "); }
            return sb.ToString();
        }

        public void purgeBySec(long sec)
        {
            var ticks = DateTime.Now.AddSeconds(-1 * sec).Ticks;
            string query = $"delete from cache where ticks < {ticks}";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "vacuum;";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
