using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    public class ParamDate : ParamFilter
    {
        static Regex ptnDate = new Regex("/(\\d{8})", RegexOptions.Compiled);
        public override void update(Param param)
        {
            //日付の解釈
            var mcDate = ptnDate.Matches(param.url);
            if (mcDate.Count > 2) {
                throw new ArgumentException("3日以上の日付指定には対応しておりません");
            }

            param.dateList.Clear();
            foreach (Match m in mcDate)
            {
                //数字のみMACを除外する
                var tmpYmd = Convert.ToUInt32(m.Groups[1].ToString(), 16);//MACアドレスとの切り分け用
                if (tmpYmd > param.macMin) { continue; }

                param.dateList.Add(Convert.ToUInt32(m.Groups[1].ToString()));
            }
            param.dateList.Sort();
        }
    }
}
