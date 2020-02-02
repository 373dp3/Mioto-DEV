using MiotoServerCuiMono.mioto_db;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServerCuiMono
{
    public class MonoDbWrapper
    {
        public static MonoDbWrapper instance { get; private set; }
        public static MonoDbWrapper getInstance(string dbPath = null)
        {
            if (instance != null) { return instance; }
            if (dbPath == null) { throw new ArgumentException("初回呼び出し時は必ずNULL以外を指定してください"); }
            instance = new MonoDbWrapper(dbPath);
            return instance;
        }
        SQLiteConnection conn;
        const string dbFilePrefix = "mioto_db";
        private MonoDbWrapper(string dbDirPath)
        {
            //mioto_db_v0.4_test.db

            conn = new SQLiteConnection(dbDirPath);
            if (new FileInfo(dbDirPath).Length == 0)
            {
                d("Create db: "+ dbDirPath);
                conn.RunInTransaction(() => {
                    conn.CreateTable<csvcash>();
                    conn.CreateTable<date2row>();
                    conn.CreateTable<latest2525>();

                    //conn.CreateTable<latestinfo>();//複合主キーを扱えないため手動にて
                    conn.Execute("CREATE TABLE [latestinfo] ([mac] INTEGER, [seq] INTEGER, [btn] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, PRIMARY KEY(mac, btn));");
                });
            }


        }

        private static void d(string msg)
        {
            Program.d(msg);
        }







        public class MaxRowId { public long rowid; };

        public ulong getMaxRowId()
        {
            try
            {
                return (ulong)conn.ExecuteScalar<long>("select max(rowid) from csvcash");
            }catch(Exception e)
            {
                return 0;
            }
        }
        public ulong getMaxDate2RowId()
        {
            try
            {
                return (ulong)conn.Table<date2row>().Max(v => v.endrow);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public long getMaxTick()
        {
            try
            {
                List<csvcash> rs = null;
                conn.RunInTransaction(() => {
                    var maxROWID = getMaxRowId();
                    rs = conn.Query<csvcash>("select * from csvcash where ROWID="+ maxROWID);
                });
                if( (rs != null) &&(rs.Count>0))
                {
                    return rs[0].ticks;
                }
                return conn.Table<csvcash>().Max(v => v.ticks);
            }catch(Exception e)
            {
                return 0;
            }
        }



    }
}
