using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace TweLitePalToSQLite
{
    public class DbWrapper
    {
        private static DbWrapper instance = null;
        public ulong beginRowId { get; private set; }
        public static DbWrapper getInstance(string dbPath = null)
        {
            if (instance != null) { return instance; }
            if (dbPath == null) { throw new ArgumentException("初回呼び出し時は必ずNULL以外を指定してください"); }
            instance = new DbWrapper(dbPath);
            return instance;
        }
        public SQLiteConnection conn = null;
        public SQLiteConnection memConn = null;

        const string dbFilePrefix = "mioto_twelite_pal_db";
        private DbWrapper(string dbDirPath)
        {
            //メモリDBを対象に処理する
            var dbfiledt = DateTime.Now;

            //末尾のスラッシュを調整
            dbDirPath = dbDirPath.Replace('\\', '/');
            if (dbDirPath[dbDirPath.Length - 1] != '/')
            {
                dbDirPath += "/";
            }
            //[TODO]sqlite-net移植時に無効化。最有効化が必要か？ doMigrateIfNeed(dbfiledt, dbDirPath);

            var dbPath = dbDirPath + dbFilePrefix + "_v0.4.db";
            var isDbExist = File.Exists(dbPath);
            if (Directory.Exists(dbDirPath) == false)
            {
                Directory.CreateDirectory(dbDirPath);
            }
            conn = new SQLiteConnection(dbPath);
            memConn = new SQLiteConnection(":memory:");


            //テーブル作成
            conn.CreateTable<TableTweLitePal>();

        }

        public void insert(TableTweLitePal pal)
        {
            conn.Insert(pal);
            /*
            var query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
            + " values ("
            + amp.mac + ", "
            + amp.ticks + ", '" + Mac2Ampere.ToCSV(amp) + "', " + FLG_AMPERE + ")";//Ah(アンペアアワー)
            conn.Execute(query);
            updateDateFlg();
            //*/
        }
    }
}
