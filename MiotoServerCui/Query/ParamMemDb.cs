using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    public class ParamMemDb : ParamFilter
    {
        static Regex ptnMem = new Regex("/mem:([^/]{1,20})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string key { get; private set; } = "";

        public override void update(Param param)
        {
            //MACの解釈
            var m = ptnMem.Match(param.url);
            if(m.Success==false) { return; }
            param.type = Param.TYPE.MEM_DB;
            param.memDbKey = m.Groups[1].ToString();
        }
    }
}
