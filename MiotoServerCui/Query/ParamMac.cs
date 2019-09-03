using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    class ParamMac : ParamFilter
    {
        static Regex ptnMac = new Regex("/([\\da-fA-F]{8})", RegexOptions.Compiled);
        public override void update(Param param)
        {
            //MACの解釈
            var mcMac = ptnMac.Matches(param.url);
            param.macList.Clear();
            foreach (Match m in mcMac)
            {
                var mac = Convert.ToUInt32(m.Groups[1].ToString(), 16);
                if (mac < param.macMin) continue;
                param.macList.Add(mac);
            }
        }
    }
}
