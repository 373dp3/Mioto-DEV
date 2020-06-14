using Microsoft.AspNetCore.Components;
using MiotoBlazorClient.Shared;
using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

            var cf = new SocketWorker(NavMgr, "config");
            await Task.WhenAny(cf.connectAsync<Config>(c =>
            {
                config = c;
                if (navMenu != null)
                {
                    navMenu.appVer = config.appVer;
                    navMenu.Reflesh();
                }
                action(c);
                Task.WhenAll(DisposeConfigSocket(cf));
            }));
            await Task.Yield();
        }

        public async Task update(NavigationManager NavMgr)
        {
            var cf = new SocketWorker(NavMgr, "config");
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(config);
                await cf.SendData(json);
                await Task.WhenAll(DisposeConfigSocket(cf));
            }
            catch (Exception e)
            {
                var txt = e.Message;
                cnt = txt.Length;
            }
        }

        private async Task DisposeConfigSocket(SocketWorker cf)
        {
            await Task.Yield();
            cf.Dispose();
        }
        public Config config { get; set; } = null;

        public int cnt { get; set; } = 0;
    }
}
