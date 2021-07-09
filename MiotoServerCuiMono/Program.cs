using MiotoServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServerCuiMono
{
    class Program
    {
        //sqlite-net をNuGetでインストールすること
        //https://github.com/praeclarum/sqlite-net
        //
        /**
         * [TODO]
         * ・実行環境がWindowsか、Monoか判定する処理(Serial切り替え用)
         * ・
         * 
         * */

        static void Main(string[] args)
        {
            OperatingSystem os = Environment.OSVersion;

            d("Platform:" + os.Platform);

            d("Create db ..");
            var db = MonoDbComSerial.getInstance();

            d("Insert");
            for(var i=0; i<3; i++)
            {
                var twe = new TweComSerialPacket();
                twe.mac = (uint)i;
                twe.key = "ct10";
                twe.csv = "csv,csv";

                db.insert(twe);
            }

            d("get");
            var ans = db.getCsv();
            d(ans);


            d("db test");
            var wrapper = MonoDbWrapper.getInstance("mioto_db_v0.4_test.db");
            d("getMaxRowId:" + wrapper.getMaxRowId());
            d("getMaxDate2RowId:" + wrapper.getMaxDate2RowId());
            d("getMaxTick:" + wrapper.getMaxTick());

        }

        public static void d(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " " + msg);
        }
    }
}
