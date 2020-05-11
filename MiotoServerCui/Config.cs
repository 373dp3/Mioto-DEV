using MiotoServer.CfgOption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static MiotoServer.MiotoServerWrapper;

namespace MiotoServer
{
    public class Config
    {
        [JsonIgnore]
        public StatusMsgFunc func { get; set; } = null;

        public const string PORT_NO_USE_KEY = "使用しない";

        public string dbdir { get; set; } = "."+ Path.DirectorySeparatorChar + "db";

        public string backupDir { get; set; } = "";

        public int hhmm { get; set; } = 500;

        public int serverPortNumber { get; set; } = 80;
        public int serverPortNumberPre { get; set; } = -1;

        public List<ComPort> listComPort { get; set; } = new List<ComPort>();


        #region 電流センサ・オンメモリDBの関連
        /// <summary>
        /// メモリDBの保持時間(秒)
        /// </summary>
        public int memoryDbPurgeSec { get; set; } = 180;
        public bool isMemoryDbBackup { get; set; } = true;
        public List<SerialCurrent> listSerialCurrent { get; set; }

        public Config()
        {
            listSerialCurrent = new List<SerialCurrent>();
        }

        public void updateSerialCurrentList(uint mac)
        {
            if (listSerialCurrent.Count(seri => seri.mac == mac) > 0) return;
            listSerialCurrent.Add(new SerialCurrent {
                mac = mac, name="", 
                listChInfo= new List<SerialCurrent.ChInfo> {
                    new SerialCurrent.ChInfo(),
                    new SerialCurrent.ChInfo(),
                    new SerialCurrent.ChInfo(),
                    new SerialCurrent.ChInfo(),
                    new SerialCurrent.ChInfo()
                }
            });
        }
        #endregion

        public static Config formJSON(string json)
        {
            try
            {
                d(json);
                return JsonSerializer.Deserialize<Config>(json);
            }
            catch (Exception e)
            {
                return new Config();
            }
        }

        public string toJSON()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
