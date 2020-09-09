using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MiotoBlazorCommon.DB;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace MiotoBlazorCommon.Struct
{
    public class ConfigTwe
    {
        [Required]
        public long mac { get; set; } = 0;

        [Required]
        [StringLength(50, ErrorMessage = "文字数が超過しています(50バイトまで)")]
        public string name { get; set; } = "";

        public long Ticks { get; set; }

        public List<ProductionConditions> listConditions { get; set; } = new List<ProductionConditions>();

        public string getConditionsTsv()
        {
            return string.Join("\r\n", listConditions.Select(q => q.ToTSV()).ToList());
        }

        public void setConditionsTsv(string tsv)
        {
            var lines = Regex.Split(tsv, "[\\r\\n]{1,100}");
            listConditions.Clear();
            if(lines.Length==0)
            {
                try
                {
                    listConditions.Add(new ProductionConditions(tsv));
                }
                catch (Exception e) { }
            }
            foreach (var line in lines)
            {
                try
                {
                    listConditions.Add(new ProductionConditions(line));
                }
                catch (Exception e) { }
            }
        }

        public void moveConditions(ProductionConditions item, bool isUp)
        {
            if (listConditions.Contains(item) == false) { return; }

            for (var i=0; i<listConditions.Count; i++)
            {
                listConditions[i].metrix = i * 2;
            }

            foreach(var obj in listConditions)
            {
                if(obj!=item) { continue; }
                if (isUp)
                {
                    obj.metrix -= 3;
                }
                else
                {
                    obj.metrix += 3;
                }
            }
            listConditions = listConditions.OrderBy(q => q.metrix).ToList();
        }

        public static ConfigTwe Convert(LastInfo info)
        {
            var ans = new ConfigTwe() { mac = info.mac, name = "", Ticks=info.ticks };
            return ans;
        }

        public static void Copy(ConfigTwe from, ConfigTwe to)
        {
            to.mac = from.mac;
            to.name = (string)from.name.Clone();
            to.setConditionsTsv(from.getConditionsTsv());
            to.Ticks = from.Ticks;
        }
            
    }
}
