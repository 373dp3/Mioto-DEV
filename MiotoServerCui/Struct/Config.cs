using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MiotoServer.DB;

namespace MiotoServer.Struct
{
    public class Config
    {
        /// <summary>
        /// 日付変更時刻
        /// </summary>
        public int dateLineHHMM { get; set; } = 500;

        public List<CtPanel2Index> listCtPanel2Index { get; set; } = new List<CtPanel2Index>();
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
    }

    public class CtPanel2Index
    {
        public ConfigCtPanel panel { get; set; } = new ConfigCtPanel();
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
