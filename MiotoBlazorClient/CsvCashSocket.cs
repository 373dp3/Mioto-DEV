using Microsoft.AspNetCore.Components;
using MiotoServer.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class CsvCashSocket : SocketWorker
    {
        public CsvCashSocket(string host)
        {
            prepare($"ws://{host}/csvcash/");
        }
        public CsvCashSocket(NavigationManager mgr)
        {
            prepare($"ws://{new Uri(mgr.Uri).Host}/csvcash/");
        }


        public async Task connectAsync(Action<CycleTime[]> action)
        {
            await connectAsync((msg) =>
            {
                var list = new List<CycleTime>();
                foreach(var line in msg.Replace('\r', '\n').Split('\n'))
                {
                    if (line.Length < 5) { continue; }
                    var ct = CycleTime.Parse(line);
                    if(ct==null) { continue; }
                    list.Add(ct);
                }

                action(list.ToArray());
            });
        }
    }
}