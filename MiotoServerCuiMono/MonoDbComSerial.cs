using MiotoServer;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServerCuiMono
{
    public class MonoDbComSerial
    {
        public static MonoDbComSerial instance { get; private set; } = null;

        public static MonoDbComSerial getInstance()
        {
            if(instance == null) { instance = new MonoDbComSerial(); }
            return instance;
        }

        SQLiteConnection conn;

        private MonoDbComSerial()
        {
            conn = new SQLiteConnection(":memory:");
            TableQuery<SerialCache> query = new TableQuery<SerialCache>(conn);

            conn.CreateTable<SerialCache>();

        }

        public void insert(TweComSerialPacket packet)
        {
            string query = "INSERT INTO SerialCache (mac, key, ticks, csv) "
                + $" values ({packet.mac}, '{packet.key}', {DateTime.Now.Ticks}, '{packet.csv}')";

            conn.Execute(query);

        }

        public string getCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("datetime,mac,ch1,ch2,ch3,ch4,ch5" + Environment.NewLine);

            var query = conn.Table<SerialCache>();
            foreach(var cache in query)
            {
                var dt = new DateTime(Convert.ToInt64(cache.ticks)).ToString("yyyy/MM/dd HH:mm:ss.fff");
                sb.Append($"{dt},{cache.mac.ToString("X")},{cache.csv}" + Environment.NewLine);
            }

            return sb.ToString();
        }


    }
}
