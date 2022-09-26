using Grapevine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweLitePalToSQLite
{
    [RestResource]
    public class MyResource
    {
        [RestRoute("Get", "/image/{deg}/{humi}")]
        public async Task Test(IHttpContext context)
        {
            var g = new WioImage();
            var deg = Convert.ToSingle(context.Request.PathParameters["deg"]);
            var humi = Convert.ToSingle(context.Request.PathParameters["humi"]);
            using (var mem = g.drawTempMonitor("製造2課 オイルパンライン", deg, humi))
            {
                Console.WriteLine($"size:{mem.Length}");

                context.Response.ContentType = "image/jpeg";
                context.Response.ContentLength64 = mem.Length;
                await context.Response.SendResponseAsync(mem.ToArray());
            }
        }
    }
}
