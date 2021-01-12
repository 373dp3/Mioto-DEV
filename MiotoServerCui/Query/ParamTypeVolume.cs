using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer.Query
{
    public class ParamTypeVolume : ParamFilter
    {
        static Regex ptnTin = new Regex("/t(\\d{1,10})s", RegexOptions.Compiled);
        static Regex ptnBlazor = new Regex("/bz(\\d{1,30})", RegexOptions.Compiled);
        static Regex ptnFix = new Regex("/fix(\\d{1,10})", RegexOptions.Compiled);
        public override void update(Param param)
        {
            if (param.url.Contains("/backup"))
            {
                param.option = Param.OPTION.BACKUP;
            }
            if (param.url.Contains("/twe"))
            {
                param.type = Param.TYPE.TWE;

                //間引き確認
                var m = ptnTin.Match(param.url);
                if (m.Success)
                {
                    param.thiningSec = Convert.ToInt32(m.Groups[1].ToString());
                    Program.d("thinning :" + param.thiningSec);
                    param.volume = Param.VOLUME.THINING;
                }
            }

            if (param.url.Contains("/pal"))
            {
                param.type = Param.TYPE.PAL;

                //間引き確認
                var m = ptnTin.Match(param.url);
                if (m.Success)
                {
                    param.thiningSec = Convert.ToInt32(m.Groups[1].ToString());
                    Program.d("thinning :" + param.thiningSec);
                    param.volume = Param.VOLUME.THINING;
                }
            }
            if (param.url.Contains("/t2525"))
            {
                param.type = Param.TYPE.TWE2525;
                //間引き処理は行わない
            }
            if (param.url.Contains("/ah"))
            {
                param.type = Param.TYPE.AH;
                //間引き確認
                var m = ptnTin.Match(param.url);
                if (m.Success)
                {
                    param.thiningSec = Convert.ToInt32(m.Groups[1].ToString());
                    Program.d("thinning :" + param.thiningSec);
                    param.volume = Param.VOLUME.THINING;
                }
            }

            if (param.url.Contains("/production_factor"))
            {
                param.type = Param.TYPE.PRODUCTION_FACTOR;
            }

            if (param.url.Contains("/final"))
            {
                if(param.volume!= Param.VOLUME.NORMAL)
                {
                    throw new ArgumentException("finalとtNNNsは同時指定ができません");
                }
                param.volume = Param.VOLUME.FINAL;
            }

            //最大数指定
            var mFix = ptnFix.Match(param.url);
            if (mFix.Success)
            {
                param.fixRow = Convert.ToInt64(mFix.Groups[1].ToString());
            }

            //Blazor Client用Ticks指定
            var mticks = ptnBlazor.Match(param.url);
            if (mticks.Success)
            {
                param.orderMinTicks = Convert.ToInt64(mticks.Groups[1].ToString());
                param.type = Param.TYPE.BLAZOR_CLIENT_POLLING;
                //Program.d("ticks :" + param.ansMaxTicks);
            }

            if (param.url.Contains("/bzconfig"))
            {
                param.type = Param.TYPE.BLAZOR_CONFIG;
            }

            if (param.url.Contains("/mac2machine"))
            {
                param.type = Param.TYPE.MAC2MACHINE;
            }

            if (param.url.Contains("/"+Config.URL_NO_LONGPOLLING_KEY))
            {
                param.isLongPolling = false;
            }
        }
    }
}
