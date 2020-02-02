/*
Copyright (c) 2019 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using MiotoServer.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    class HttpdWorker
    {
        static TwePacketParser parser = new TwePacketParser();
        static HttpListener listener = null;
        static Thread httpTh = null;
        static bool isThreadEnable = false;

        public HttpdWorker()
        {
            paramFilterList.Add(new ParamDate());
            paramFilterList.Add(new ParamMac());
            paramFilterList.Add(new ParamTypeVolume());
            paramFilterList.Add(new ParamMemDb());
        }
        public void httpdRestart()
        {
            Program.d("httpd restart required");
            if (httpTh == null) { return; }
            if(listener==null) { return; }
            isThreadEnable = false;
            try
            {
                listener.Stop();
                httpTh.Join(30 * 1000);
            }catch(ThreadStateException te)
            {
                Program.d("ThreadStateExcepton on httpdRestart:"+te.ToString());
            }catch(HttpListenerException le)
            {
                Program.d("HttpListenerException on httpdRestart:" + le.ToString());
            }
            httpTh = null;
            Run();
        }
        public void Run()
        {
            try
            {
                Program.d("Starting httpd");
                DbWrapper dbWrapper = DbWrapper.getInstance();
                string httpdPrefix = "http://*:80/";
                listener = new HttpListener();
                listener.Prefixes.Add(httpdPrefix);
                listener.Start();
                isThreadEnable = true;
                const string IF_MOD_SINCE = "If-Modified-Since";
                httpTh = new Thread(() => {
                    Program.d("Httpd working ... ");
                    while (Program.isActive && isThreadEnable)
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

                            //Last-Modifiedヘッダの適用
                            var headers = res.Headers;
                            headers.Add(HttpResponseHeader.LastModified,
                                dbWrapper.getLastUpdateHttpdFormat());

                            //音声再生リクエストの判定と処理
                            if (isSoundRequest(context)) { continue; }

                            //Not Modifiedレスポンス処理
                            if (reqHeaders.AllKeys.Contains(IF_MOD_SINCE)
                                && (!dbWrapper.isModified(reqHeaders.Get(IF_MOD_SINCE))))
                            {
                                res.StatusCode = 304;
                                res.Close();
                                Program.d("304 Not modified");
                                continue;
                            }

                            /*/ //各種リクエストヘッダをコンソールに出力する場合には内部を有効化
                            foreach (var key in reqHeaders.AllKeys)
                            {
                                Program.d("\t" + key + "\t" + reqHeaders.Get(key));
                            }
                            //*/

                            //Excel等では最初にHEADメソッドを実行するため
                            if (context.Request.HttpMethod.CompareTo("HEAD") == 0)
                            {

                                res.Close();
                                Program.d("http head");
                                continue;
                            }

                            //POSTはESPからの情報アップロード
                            if (context.Request.HttpMethod.CompareTo("POST") == 0)
                            {
                                Program.d("Method post");
                                using (var postBody = new StreamReader(context.Request.InputStream))
                                {
                                    var body = postBody.ReadToEnd();
                                    Program.d(body);
                                    //Serialと共通の解析処理へ転送
                                    parser.parse(body, 8);//ESPの場合、8ビット目まで入力と判定
                                }
                                res.Close();
                                continue;
                            }

                            //DB参照リクエスト
                            byte[] content = getContents(dbWrapper, context);
                            if(content == null)
                            {
                                res.StatusCode = 400;
                            }else
                            {
                                res.OutputStream.Write(content, 0, content.Length);
                            }
                            res.Close();
                        }
                        catch (ProtocolViolationException pe)
                        {
                            Program.d("Error: " + pe.ToString());
                        }catch (HttpListenerException le)
                        {
                            Program.d("HttpListenerを停止しました");
                        }
                        catch (Exception ee)
                        {
                            Program.d("Error: " + ee.ToString());
                        }
                    }
                    Program.d("Httpd is shutting down ... ");
                    listener.Prefixes.Remove(httpdPrefix);
                });
                httpTh.Priority = ThreadPriority.BelowNormal;
                httpTh.Start();
            }
            catch (Exception e)
            {
                Program.d("Error:" + e.ToString());
            }

        }


        private static BlockingCollection<ParamFilter> paramFilterList = new BlockingCollection<ParamFilter>();

        private static Regex ptnSoundFile = new Regex("(.*)\\.(mp3|wav)", RegexOptions.Compiled);

        private static bool isSoundRequest(HttpListenerContext context)
        {
            /**
             * http://localhost/sound/filename.(mp3|wav)/sessionid
             * */
            if(context.Request.HttpMethod.CompareTo("HEAD")==0) { return false; }
            var url = context.Request.RawUrl.ToLower();
            if (url.Contains("sound") == false) { return false; }
            if (url.Contains("\\")) { return false; }//\を含むURLは処理しない。
            var ary = context.Request.RawUrl.Split('/');

            var listSoundFile = new List<string>();
            foreach(var node in ary)
            {
                var m = ptnSoundFile.Match(node);
                if(m.Success==false) { continue; }
                listSoundFile.Add(m.Groups[0].Value);
                Program.d("sound:" + m.Groups[0].Value);
            }

            HttpListenerResponse res = context.Response;

            if (listSoundFile.Count == 0)
            {
                res.StatusCode = 404;
                res.Close();
            }
            else
            {
                var headers = context.Response.Headers;
                headers.Add(HttpResponseHeader.ContentType, "text/plain; charset=UTF-8");
                res.StatusCode = 200;

                var p = DbSoundOrder.getInstance();
                foreach(var soundFile in listSoundFile)
                {
                    p.insertOrUpdateFile(soundFile);
                }
                byte[] content = null;
                content = Encoding.UTF8.GetBytes("file: "+string.Join(", ", listSoundFile));
                res.OutputStream.Write(content, 0, content.Length);
                res.Close();
            }



            return true;
        }

        private static byte[] getContents(DbWrapper dbWrapper, HttpListenerContext context)
        {
            var headers = context.Response.Headers;
            headers.Add(HttpResponseHeader.ContentType, "text/plain; charset=UTF-8");

            //大文字・小文字のゆらぎ吸収
            var param = new Param(context.Request.RawUrl.ToLower());
            try
            {
                foreach (var f in paramFilterList)
                {
                    f.update(param);
                }
            }
            catch (ArgumentException ar)
            {
                context.Response.StatusCode = 400;
                return Encoding.UTF8.GetBytes(ar.Message);
            }

            var paramStr = param.ToString();

            Program.d("get csv " + paramStr);
            byte[] content = null;
            switch (param.type)
            {
                case Param.TYPE.MEM_DB:
                    content = Encoding.UTF8.GetBytes(DbComSerial.getInstance().getCsv(param));
                    break;

                default:
                    content = Encoding.UTF8.GetBytes(dbWrapper.getCsv(param));
                    break;
            }

            //ダウンロード対応
            if (param.url.Contains("/download"))
            {
                headers.Add("Content-Disposition", "attachment;filename=\"mioto_" 
                    + paramStr + ".csv\"");
            }

            return content;
        }
    }
}
