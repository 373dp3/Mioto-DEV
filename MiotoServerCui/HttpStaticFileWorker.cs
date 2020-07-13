using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiotoServer
{
    class HttpStaticFileWorker
    {
#if DEBUG
        public string wwwroot { get; set; } = "..\\..\\..\\MiotoBlazorClient\\wwwroot\\";
        public string frameworkRoot { get; set; } = "..\\..\\..\\MiotoBlazorClient\\bin\\Debug\\netstandard2.1\\wwwroot\\";
#else
        public string wwwroot { get; set; } = ".\\html\\";
        public string frameworkRoot { get; set; } = ".\\html\\";
#endif
        private static Regex ptn = new Regex("\\/html\\/(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Dictionary<string, string> dictMIME { get; set; } = new Dictionary<string, string>()
        {
            {".aac", "audio/aac"},
            {".abw", "application/x-abiword"},
            {".arc", "application/x-freearc"},
            {".avi", "video/x-msvideo"},
            {".azw", "application/vnd.amazon.ebook"},
            {".bin", "application/octet-stream"},
            {".bmp", "image/bmp"},
            {".bz", "application/x-bzip"},
            {".bz2", "application/x-bzip2"},
            {".csh", "application/x-csh"},
            {".css", "text/css"},
            {".csv", "text/csv"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".eot", "application/vnd.ms-fontobject"},
            {".epub", "application/epub+zip"},
            {".gz", "application/gzip"},
            {".gif", "image/gif"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/vnd.microsoft.icon"},
            {".ics", "text/calendar"},
            {".jar", "application/java-archive"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "text/javascript"},
            {".json", "application/json"},
            {".jsonld", "application/ld+json"},
            {".mid", "audio/midi audio/x-midi"},
            {".midi", "audio/midi audio/x-midi"},
            {".mjs", "text/javascript"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpkg", "application/vnd.apple.installer+xml"},
            {".odp", "application/vnd.oasis.opendocument.presentation"},
            {".ods", "application/vnd.oasis.opendocument.spreadsheet"},
            {".odt", "application/vnd.oasis.opendocument.text"},
            {".oga", "audio/ogg"},
            {".ogv", "video/ogg"},
            {".ogx", "application/ogg"},
            {".opus", "audio/opus"},
            {".otf", "font/otf"},
            {".png", "image/png"},
            {".pdf", "application/pdf"},
            {".php", "application/x-httpd-php"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".rar", "application/vnd.rar"},
            {".rtf", "application/rtf"},
            {".sh", "application/x-sh"},
            {".svg", "image/svg+xml"},
            {".swf", "application/x-shockwave-flash"},
            {".tar", "application/x-tar"},
            {".tif", "image/tiff"},
            {".tiff", "image/tiff"},
            {".ts", "video/mp2t"},
            {".ttf", "font/ttf"},
            {".txt", "text/plain"},
            {".vsd", "application/vnd.visio"},
            {".wav", "audio/wav"},
            {".weba", "audio/webm"},
            {".webm", "video/webm"},
            {".webp", "image/webp"},
            {".woff", "font/woff"},
            {".woff2", "font/woff2"},
            {".xhtml", "application/xhtml+xml"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".xml", "application/xml"},
            {".xul", "application/vnd.mozilla.xul+xml"},
            {".zip", "application/zip"},
            {".7z", "application/x-7z-compressed"},
        };
        public bool doOperateIfStaticFileRequest(HttpListenerContext context, HttpListenerResponse res)
        {
            var m = ptn.Match(context.Request.Url.AbsoluteUri);
            var root = wwwroot;
            if (m.Success == false) { return false; }

            if (m.Groups[1].Value.Contains("_framework"))
            {
                root = frameworkRoot;
            }

            var fileOrder = (root + m.Groups[1]).Replace('/', '\\');
            if (m.Groups[1].Value.Length == 0)
            {
                fileOrder += "index.html";
            }

            //ファイルが見つからない場合の処理 [TODO] Blazer向けURLリライト処理が後々必要になる
            var fileInfo = new FileInfo(fileOrder);
            if (fileInfo.Exists == false)
            {
                //ファイルの拡張子がない場合は、Blazor WebAssemblyの識別子だと判断する
                if (fileInfo.Extension.Length == 0)
                {
                    fileOrder = root + "index.html";
                    fileInfo = new FileInfo(fileOrder);
                }
                else
                {
                    res.StatusCode = 404;
                    res.Close();
                    return true;
                }
            }

            //ファイルロード、転送
            try
            {
                res.StatusCode = 200;
                if (dictMIME.ContainsKey(fileInfo.Extension))
                {
                    res.ContentType = dictMIME[fileInfo.Extension];
                }
                byte[] content = File.ReadAllBytes(fileOrder);
                res.OutputStream.Write(content, 0, content.Length);
            }
            catch (Exception e)
            {
                res.StatusCode = 500; // 404 でも良いのだがここは雑に 500 にまとめておく
                byte[] content = Encoding.UTF8.GetBytes(e.Message);
                res.OutputStream.Write(content, 0, content.Length);
            }
            res.Close();
            return true;

        }

        private static void d(string msg)
        {
            Program.d(msg);
        }

    }
}