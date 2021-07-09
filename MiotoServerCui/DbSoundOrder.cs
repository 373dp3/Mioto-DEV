using MiotoServer.DB;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMPLib;

namespace MiotoServer
{
    public class DbSoundOrder
    {
        public static DbSoundOrder instance { get; private set; } = null;
        public long preTick { get; private set; } = DateTime.Now.Ticks;

        public string soundDir { get; set; } = "";

        public static DbSoundOrder getInstance()
        {
            if(instance==null) { instance = new DbSoundOrder(); }
            return instance;
        }       

        public void setDataPath(string path)
        {
            soundDir = path.Replace('/','\\');
            soundDir = string.Join("\\", soundDir.Split('\\'));
            soundDir +=  "\\alarm_mp3";
            soundDir = soundDir.Replace('\\', Path.DirectorySeparatorChar);
            if (System.IO.Directory.Exists(soundDir) == false)
            {
                System.IO.Directory.CreateDirectory(soundDir);
            }
            Program.d("sound dir: " + soundDir);
        }

        SQLiteConnection conn = null;

        private DbSoundOrder()
        {
            conn = new SQLiteConnection(":memory:");
            conn.CreateTable<SoundTbl>();
        }

        public void insertOrUpdateFile(string soundFile)
        {
            string query = "INSERT OR REPLACE INTO SoundTbl (file, ticks) "
                + $" values ('{soundFile}', {DateTime.Now.Ticks})";
            conn.Execute(query);
        }

        public long getTick(string soundFile)
        {
            try
            {
                return conn.ExecuteScalar<long>("select ticks from SoundTbl where file='{soundFile}'");

            }catch(Exception e)
            {
                return 0;
            }
        }

        public List<string> getSoundFiles(long prevTick)
        {
            var rs = conn.Table<SoundTbl>().Where(v => v.ticks >= prevTick);
            List<string> list = new List<string>();
            foreach(var item in rs)
            {
                if (item.file.Length == 0) { continue; }
                list.Add(item.file);
            }
            return list;
        }

        private bool isPlaing = false;
        public void fetch()
        {
            if(isPlaing) { return; }
            if(instance==null) { return; }
            var invokeTick = DateTime.Now.Ticks;
            var files = getSoundFiles(preTick);
            isPlaing = true;
            preTick = invokeTick;

            bool isAlarmPlay = false;
            foreach (var file in files)
            {
                if (isAlarmPlay == false)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    Thread.Sleep(3000);
                    isAlarmPlay = true;
                }
                var soundPath = soundDir + Path.DirectorySeparatorChar + file;
                if (System.IO.File.Exists(soundPath))
                {
                    for(int i=0; i<3; i++)
                    {
                        try
                        {
                            Program.d("sound play: " + file);
                            var player = new WindowsMediaPlayer();
                            player.URL = soundPath;
                            player.controls.play();
                            while (player.playState == WMPPlayState.wmppsTransitioning)
                            {
                                Thread.Sleep(200);
                            }
                            while (player.playState == WMPPlayState.wmppsPlaying)
                            {
                                Thread.Sleep(200);
                            }
                            break;
                        }
                        catch (Exception se)
                        {
                            Program.d(se.ToString());
                        }
                    }
                }
                else
                {
                    Program.d("sound file not found: " + file);
                }
            }
            isPlaing = false;
        }


    }
}
