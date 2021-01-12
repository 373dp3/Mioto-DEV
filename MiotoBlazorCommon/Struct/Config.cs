using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MiotoBlazorCommon.DB;

namespace MiotoBlazorCommon.Struct
{
    public class Config
    {
        /// <summary>
        /// 日付変更時刻
        /// </summary>
        public string appVer { get; set; } = "";
        public int dateLineHHMM { get; set; } = 500;

        public List<CtPanel2Index> listCtPanel2Index { get; set; } = new List<CtPanel2Index>();
        public List<GanttPanel2Index> listGanttPanel2Index { get; set; } = new List<GanttPanel2Index>();
        public List<ConfigTwe> listTwe { get; set; } = new List<ConfigTwe>();

        public void insertOrUpdateTwe(List<LastInfo> list)
        {
            if(list == null) { return; }
            foreach(var item in list)
            {
                if (listTwe.Where(q => q.mac == item.mac).Count() > 0) { continue; }
                var twe = ConfigTwe.Convert(item);
                listTwe.Add(ConfigTwe.Convert(item));
            }
        }

        public ConfigAlert alert = new ConfigAlert();

        public const string RELOAD_FORCE_KEY = "reload_force_key";

        public const int POLLING_LONGPOLLING = -1;
        public const string URL_NO_LONGPOLLING_KEY = "no_longpolling";
    }

    public class CtPanel2Index
    {
        public ConfigCtPanel panel { get; set; } = new ConfigCtPanel();
        public long index { get; set; } = 0;
    }

    public class GanttPanel2Index
    {
        public ConfigGanttPanel panel { get; set; } = new ConfigGanttPanel();
        public long index { get; set; } = 0;
    }

    public class ConfigAlert
    {
        public float battRed { get; set; } = 2.3f;
        public float battGreen { get; set; } = 2.7f;
        public byte lqiRed { get; set; } = 30;
        public byte lqiGreen { get; set; } = 70;

    }
}
