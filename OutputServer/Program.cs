using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace OutputServer
{
    /*
     * LibreCalcなどから、webservice関数を用いてアクセスさせることで、
     * 外部出力を可能にするためのソフトウェア試作。
     * 音声ファイルの再生程度を実施。
     * 
     * */
    class Program
    {
        static SoundPlayer player = null;
        public static bool isActive = true;
        static void Main(string[] args)
        {
            Play();
            var httpd = new HttpdWorker();
            httpd.Run();

            Console.WriteLine("-- Hit any key and end. --");
            Console.ReadKey();
            isActive = false;

        }

        public static void d(string msg)
        {
            Console.WriteLine(msg);
            //Trace.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " " + msg);
        }

        public static void Play()
        {
            string file = @"‪‪‪sound\1.wav";
            FileInfo fi = new FileInfo(file);
            if (player!=null) {
                Console.WriteLine("Player instance is not null");
                return;
            }
            try
            {
                Console.WriteLine("Play sound.");
                player = new SoundPlayer(fi.FullName);
                player.PlaySync();
                player.Dispose();
                player = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
 
        }
    }
}
