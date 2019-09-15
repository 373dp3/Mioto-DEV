using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

/**
 * こちらのコードは複数ファイル用に修正を実施。
 * オリジナルのコードは、C:\Users\min\Desktop\IoT講習資料\20190716_ソフトウェア技術者IoT開発\0716_IoT\source
 * を参照の事
 * */


namespace CalcHelper
{
    class HttpdWorker
    {
        static HttpListener listener = null;
        public static Thread httpTh { get; private set; }
        static bool isThreadEnable = false;

        const string HTML_DIR = "./html/";
        const string INDEX_HTML = "index.html";
        static string indexHtml = "";

        public void httpdRestart()
        {
            Form1.log("httpd restart required");
            Stop();
            Run();
        }
        public void Stop()
        {
            if (httpTh == null) { return; }
            if (listener == null) { return; }
            isThreadEnable = false;
            try
            {
                listener.Stop();
                httpTh.Join(5 * 1000);
            }
            catch (ThreadStateException te)
            {
                Form1.log("ThreadStateExcepton on httpdRestart:" + te.ToString());
            }
            catch (HttpListenerException le)
            {
                Form1.log("HttpListenerException on httpdRestart:" + le.ToString());
            }
            httpTh = null;

        }
        public void Run()
        {
            var assy = Assembly.GetEntryAssembly();
            var fi = new FileInfo(assy.Location);
            var dirPath = fi.Directory;


            //index.htmlのロード
            using (var sr = new StreamReader(HTML_DIR + INDEX_HTML,
                System.Text.Encoding.UTF8))
            {
                indexHtml = sr.ReadToEnd();
            }


            //サービス開始
            try
            {
                Form1.log("Starting httpd");
                string httpdPrefix = "http://*:80/img/";
                listener = new HttpListener();
                listener.Prefixes.Add(httpdPrefix);
                
                listener.Start();
                isThreadEnable = true;

                httpTh = new Thread(() => {
                    Form1.log("Httpd working ... ");
                    while (Form1.isActive && isThreadEnable)
                    {
                        try
                        {
                            HttpListenerContext context = listener.GetContext();
                            HttpListenerResponse res = context.Response;
                            var reqHeaders = context.Request.Headers;
                            if (context.Request.RawUrl.Contains(".ico"))
                            {
                                res.StatusCode = 404;
                                res.Close();
                                continue;
                            }
                            res.StatusCode = 200;

                            

                            byte[] content = getContents(context);
                            if (content == null)
                            {
                                res.StatusCode = 400;
                            }
                            else
                            {
                                res.OutputStream.Write(content, 0, content.Length);
                            }
                            res.Close();
                        }
                        catch (ProtocolViolationException pe)
                        {
                            Form1.log("Error: " + pe.ToString());
                        }
                        catch (HttpListenerException le)
                        {
                            le.ToString();
                            Form1.log("HttpListenerを停止しました:");
                        }
                        catch (Exception ee)
                        {
                            Form1.log("Error: " + ee.ToString());
                        }
                    }
                    Form1.log("Httpd is shutting down ... ");
                    listener.Prefixes.Remove(httpdPrefix);
                });
                httpTh.Priority = ThreadPriority.BelowNormal;
                httpTh.Start();
            }
            catch (Exception e)
            {
                Form1.log("Error:" + e.ToString());
            }

        }

        private static void log(string msg)
        {
            Form1.log(msg);
        }

        static Regex ptnImgArg = new Regex("/img/(.*)", RegexOptions.Compiled);
        static Regex ptnImgCapArg = new Regex("/img/capture/(.*)", RegexOptions.Compiled);
        static Regex ptnExt = new Regex(".*\\.([\\w\\d]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static byte[] getContents(HttpListenerContext context)
        {
            var headers = context.Response.Headers;

            //大文字・小文字のゆらぎ吸収
            var param = context.Request.RawUrl.ToLower();
            if (param.CompareTo("/img/capture") == 0)
            {
                return getImage(Form1.DEFAULT_IMG_NAME, context);
            }
            var im = ptnImgCapArg.Match(param);
            if (im.Success)
            {
                var img = getImage(im.Groups[1].Value.ToString(), context);
                if(img != null) { return img; }
                log("img not found");
                return getImage(Form1.DEFAULT_IMG_NAME, context);
            }

            //静的ファイル参照
            var m = ptnImgArg.Match(param);
            if (m.Success==false)
            {
                headers.Add(HttpResponseHeader.ContentType, "text/plain; charset=UTF-8");
                context.Response.StatusCode = 404;
                return Encoding.UTF8.GetBytes("File not found");
            }

            var path = HTML_DIR + m.Groups[1].ToString();
            if (m.Groups[1].ToString().Length == 0)
            {
                path += INDEX_HTML;
            }

            // index.html以外の処理
            if (path.Contains(HTML_DIR + INDEX_HTML) == false)
            {
                return getFileByte(path, param, headers, context);
            }

            // index.htmlの動的生成
            var html = String.Copy(indexHtml);
            var files = MemoryDb.getInstance().getImageNames();
            var imgListTag = "";
            foreach(var file in files) {
                imgListTag += HttpUtility.UrlDecode(file) + "\t" + file + Environment.NewLine;
                //log("db files: " + file + " // " + ;
            }

            byte[] content = Encoding.UTF8.GetBytes(html.Replace("__IMAGE_LIST__", imgListTag));
            
            headers.Add(HttpResponseHeader.ContentType, getContentType("html"));
            
            return content;
        }

        private static byte[] getFileByte(string path, string param, WebHeaderCollection headers, HttpListenerContext context)
        {
            var m2 = ptnExt.Match(param);
            var ext = "";
            if (m2.Success)
            {
                ext = m2.Groups[1].ToString().ToLower();
                //log("ext: " + m2.Groups[1].ToString());
            }

            var files = MemoryDb.getInstance().getImageNames();
            //foreach (var file in files) { log("db files: " + file + " // " + HttpUtility.UrlDecode(file)); }

            var fi = new FileInfo(path);
            if (fi.Exists == false)
            {
                headers.Add(HttpResponseHeader.ContentType, getContentType(ext));
                context.Response.StatusCode = 404;
                return Encoding.UTF8.GetBytes("File not found");
            }

            byte[] content = null;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var r = new BinaryReader(fs))
            {
                content = r.ReadBytes((int)fi.Length);
            }

            headers.Add(HttpResponseHeader.ContentType, getContentType(ext));

            return content;
        }

        private static byte[] getImage(string name, HttpListenerContext context)
        {
            var headers = context.Response.Headers;
            var content = MemoryDb.getInstance().getImage(name);
            if (content == null)
            {
                headers.Add(HttpResponseHeader.ContentType, "text/plain; charset=UTF-8");
                context.Response.StatusCode = 400;
                return Encoding.UTF8.GetBytes("ファイルが見つかりませんでした。");
            }
            return content;
        }

        static Dictionary<string, string> dictContntType = new Dictionary<string, string>()
        {
            {"html", "text/html"},
            {"htm", "text/html"},
            {"js", "text/javascript"},
            {"json", "application/json"},
            {"jpg", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"png", "image/png"},
            {"gif", "image/gif"},
            {"css", "text/css"}
        };
        private static string getContentType(string ext)
        {
            var ans = dictContntType[ext];
            if (ans == null)
            {
                return "application/octet-stream";
            }
            return ans;
        }
    }
}
