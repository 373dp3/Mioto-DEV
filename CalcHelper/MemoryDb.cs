using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcHelper
{
    class MemoryDb
    {
        private static MemoryDb instance = null;
        public static MemoryDb getInstance()
        {
            if (instance == null) instance = new MemoryDb();
            return instance;
        }
        SQLiteConnection conn = null;
        string[] createQueryAry = {
                //(UInt32 mac, byte seq, byte btn, byte lqi, UInt16 batt, long ticks)
                "CREATE TABLE [imgcash] ([name] TEXT primary key, [ticks] INTEGER, [img] BLOB);"
            };

        private MemoryDb()
        {
            var sqlConnectionSb = new SQLiteConnectionStringBuilder
            {
                DataSource = ":memory:"
            };
            conn = new SQLiteConnection(sqlConnectionSb.ToString());
            conn.Open();

            using (var tran = conn.BeginTransaction())
            {
                foreach (var query in createQueryAry)
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();

                }
                tran.Commit();
            }
        }

        public void insertImage(string name, byte[] imgData)
        {
            string query = "INSERT OR REPLACE INTO imgcash (name, ticks, img) values (@name, @ticks, @img)";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.Add("name", DbType.String);
                cmd.Parameters.Add("ticks", DbType.UInt64);
                cmd.Parameters.Add("img", DbType.Binary);
                cmd.Parameters["name"].Value = name.ToLower();
                cmd.Parameters["ticks"].Value = DateTime.Now.Ticks;
                cmd.Parameters["img"].Value = imgData;
                cmd.ExecuteNonQuery();
            }
        }

        public List<string> getImageNames()
        {
            var list = new List<string>();
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "select name from imgcash ";
                using (var reader = readCmd.ExecuteReader())
                using (var dt = new DataTable())
                {
                    dt.Load(reader);
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add((string)row.ItemArray[0]);
                    }
                }
            }

            return list;
        }

        public byte[] getImage(string name)
        {
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "select img from imgcash where name ='"+name.ToLower()+"'";
                using (var reader = readCmd.ExecuteReader())
                using(var dt = new DataTable())
                {
                    dt.Load(reader);
                    if(dt.Rows.Count!=1) { return null; }
                    return (byte[])dt.Rows[0].ItemArray[0];
                }
            }
        }
    }
}
