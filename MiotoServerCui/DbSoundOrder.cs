using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
            if (System.IO.Directory.Exists(soundDir) == false)
            {
                System.IO.Directory.CreateDirectory(soundDir);
            }
            Program.d("sound dir: " + soundDir);
        }

        SQLiteConnection conn = null;

        private DbSoundOrder()
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
                cmd.CommandText = "CREATE TABLE [soundTbl] ([file] TEXT, [ticks] INTEGER, PRIMARY KEY(file)) ";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
        }
        public void insertOrUpdateFile(string soundFile)
        {
            string query = "INSERT OR REPLACE INTO soundTbl (file, ticks) "
                + $" values ('{soundFile}', {DateTime.Now.Ticks})";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
        }

        public long getTick(string soundFile)
        {
            long tick = 0;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = $"select ticks from soundTbl where file='{soundFile}'";
                using (var reader = readCmd.ExecuteReader())
                {
                    for (var i = 0; reader.Read(); i++)
                    {
                        if (reader[0].ToString().Length == 0) continue;
                        tick = Convert.ToInt64(reader[0]);
                        break;
                    }
                }
            }
            return tick;
        }

        public List<string> getSoundFiles(long prevTick)
        {
            List<string> list = new List<string>();
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = $"select file from soundTbl where ticks>='{prevTick}'";
                using (var reader = readCmd.ExecuteReader())
                {
                    for (var i = 0; reader.Read(); i++)
                    {
                        if (reader[0].ToString().Length == 0) continue;
                        list.Add(reader[0].ToString());
                    }
                }
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
                var soundPath = soundDir + "\\" + file;
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
