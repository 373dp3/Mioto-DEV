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
    }

    public class CtPanel2Index
    {
        public ConfigCtPanel panel { get; set; } = new ConfigCtPanel();
        public long index { get; set; } = 0;
    }
}
