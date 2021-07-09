using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OutputServer
{
    class HttpdWorker
    {
        static HttpListener listener = null;

        public void Run()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://*:80/");
                listener.Start();

                var httpTh = new Thread(() => {
                    while (Program.isActive)
                    {
                        try
                        {
                            HttpListenerContext context = listener.GetContext();
                            HttpListenerResponse res = context.Response;
                            if (context.Request.RawUrl.Contains(".ico"))
                            {
                                res.StatusCode = 404;
                                res.Close();
                                continue;
                            }
                            res.StatusCode = 200;

                            //Excel等では最初にHEADメソッドを実行するため
                            if (context.Request.HttpMethod.CompareTo("HEAD") == 0)
                            {
                                res.Close();
                                continue;
                            }

                            var uris = context.Request.RawUrl.Split('/');

                            new Thread(() => { Program.Play(); }).Start();

                            byte[] content = Encoding.UTF8.GetBytes("Play sound.");
                            res.OutputStream.Write(content, 0, content.Length);
                            res.Close();
                        }
                        catch (System.Net.ProtocolViolationException pe)
                        {
                            Program.d("Error: " + pe.ToString());
                        }
                        catch (Exception ee)
                        {
                            Program.d("Error: " + ee.ToString());
                        }
                    }
                });
                httpTh.Priority = ThreadPriority.BelowNormal;
                httpTh.Start();
            }
            catch (Exception e)
            {
                Program.d("Error:" + e.ToString());
            }

        }
    }

}
