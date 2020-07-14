using Microsoft.AspNetCore.Components;
using MiotoBlazorClient.Shared;
using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class ConfigSingleton
    {
        public NavMenu navMenu { get; set; } = null;
        private static ConfigSingleton instance = null;
        private ConfigSingleton() { }
        public static ConfigSingleton getInstance()
        {
            if (instance == null) instance = new ConfigSingleton();
            return instance;
        }
        public void setNavMenu(NavMenu menu)
        {
            this.navMenu = menu;
        }
        public async Task getConfigAsync(NavigationManager NavMgr, Action<Config> action, bool forceNew=false)
        {
            await Task.Yield();
            if((config != null) && (forceNew==false)) { 
                action(config);
                return;
            }
            
            var http = new HttpClient();
            var url = $"http://{new Uri(NavMgr.Uri).Host}/{URI_PREFIX}/_{DateTime.Now.Ticks}";
            try
            {
                var jsonStr = await http.GetStringAsync(url);
                if (jsonStr != null)
                {
                    var c = JsonSerializer.Deserialize<Config>(jsonStr);
                    config = c;
                    if (navMenu != null)
                    {
                        navMenu.appVer = config.appVer;
                        navMenu.Reflesh();
                    }
                    action(c);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetBzConfig Exception: "+e.ToString());
            }

            await Task.Yield();
        }

        public const string URI_PREFIX = "bzconfig";

        public async Task update(NavigationManager NavMgr)
        {
            var url = $"http://{new Uri(NavMgr.Uri).Host}/{URI_PREFIX}/_{DateTime.Now.Ticks}";
            var response = await new HttpClient().PostAsJsonAsync(url, config);
        }

        public Config config { get; set; } = null;

    }
}
