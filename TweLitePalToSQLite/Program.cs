// See https://aka.ms/new-console-template for more information
using Grapevine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Drawing.Text;
using System.Linq;

/*
 * 残件整理
 * must
 * ・MACと設備名称との対応
 * ・IPとMACとの対応
 * ・DBへの格納と該当情報の抽出
 * 
 * must wio terminal
 * ・固定IP対応
 * ・WiFi接続解除対応
 * ・画像ロード失敗対応
 * ・WDT処理
 * 
 * want
 * ・PALを受信した時点で画像を更新しDBに格納
 * ・上下キーでページ送り
 * 
 * */


TweLitePalToSQLite.DbWrapper.getInstance("./");

#if false
var g = new WioImage();
using (var mem = g.drawTempMonitor("製造2課 オイルパンライン", 29.8f, 22.0f))
{
    Console.WriteLine($"size:{mem.Length}");
    using(var fs = new FileStream("sample1.jpg", FileMode.Create, FileAccess.Write))
    {
        fs.Write(mem.ToArray());
    }
}
using (var mem = g.drawTempMonitor("製造2課 オイルパンライン", 29.8f, 62.0f))
{
    Console.WriteLine($"size:{mem.Length}");
    using (var fs = new FileStream("sample2.jpg", FileMode.Create, FileAccess.Write))
    {
        fs.Write(mem.ToArray());
    }
}
using (var mem = g.drawTempMonitor("製造2課 オイルパンライン", 29.8f, 82.0f))
{
    Console.WriteLine($"size:{mem.Length}");
    using (var fs = new FileStream("sample3.jpg", FileMode.Create, FileAccess.Write))
    {
        fs.Write(mem.ToArray());
    }
}
using (var mem = g.drawTempMonitor("製造2課 オイルパンライン", 31.8f, 90.0f))
{
    Console.WriteLine($"size:{mem.Length}");
    using (var fs = new FileStream("sample4.jpg", FileMode.Create, FileAccess.Write))
    {
        fs.Write(mem.ToArray());
    }
}
using (var mem = g.drawTempMonitor("第2会議室", 36.5f, 46.4f))
{
    Console.WriteLine($"size:{mem.Length}");
    using (var fs = new FileStream("sample5.jpg", FileMode.Create, FileAccess.Write))
    {
        fs.Write(mem.ToArray());
    }
}
using (var mem = g.drawTempMonitor("アイドラ研磨ライン", 28.7f, 63.9f))
{
    Console.WriteLine($"size:{mem.Length}");
    using (var fs = new FileStream("sample6.jpg", FileMode.Create, FileAccess.Write))
    {
        fs.Write(mem.ToArray());
    }
}
#endif


var ct = new CancellationTokenSource();
var token = ct.Token;
var httpdTask = Task.Run(() =>
{

    var serverBuilder = RestServerBuilder.UseDefaults();
    serverBuilder.ConfigureServer = (server) =>
    {
        server.Prefixes.Add("http://*:1234/");
    };
    using (var server = serverBuilder.Build())
    {
        
        server.Start();

        Console.WriteLine("GrapeVine http server start.");
        while (token.IsCancellationRequested == false) Thread.Sleep(1000);
        Console.WriteLine("GrapeVine http server shutting down.");
    }
    Console.WriteLine("GrapeVine http server finished.");
});

while (true)
{
    try
    {
        using (var com = new ComportHelper())
        {
            var res = com.checkPort(args) ? "hit" : "miss";
            Console.WriteLine(res);
            com.receivers.Add(new PalParser());
            com.Open();
            Console.WriteLine($"Listening port {com} ... ");

            Console.WriteLine("--hit Enter key and exit--");
            Console.ReadLine();
            ct.Cancel();
            break;
        }
    }
    catch (Exception e) { }
}

httpdTask.Wait();
Console.WriteLine("finished all task.");




