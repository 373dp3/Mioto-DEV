using KmeansTool.Table;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MathNet.Numerics.Statistics;

namespace KmeansTool
{
    public class MemDb : GuiCommon, IDisposable
    {
        SQLiteConnection conn;
        public MemDb()
        {
            conn = new SQLiteConnection(":memory:");
            //if(File.Exists("mydata.db")) { File.Delete("mydata.db"); }
            //conn = new SQLiteConnection("mydata.db");
            conn.CreateTable<CsvRow>();
        }

        public void insertCsvLine(string csv, int upperLimitDSec)
        {
            var row = CsvRow.FromCsv(csv);
            if (row == null) { d("line is null"); return; }
            if (row.ct > upperLimitDSec) { return; }
            if (row.ct < 10) { return; }//1秒未満はチャタリングとして破棄
            conn.Insert(row);
        }

        public async void load(string file, CancellationToken token, int fileLoadPercent = 100)
        {
            await Task.Run(() =>
            {
                var start = DateTime.Now;
                fetch(file, token, fileLoadPercent);
                d("time:" +(DateTime.Now-start).TotalSeconds.ToString("F1"));
            });
        }

        private void fetch(string inputFile, CancellationToken token, int fileLoadPercent = 100)
        {
            conn.BeginTransaction();
            try
            {
                insertMemory(inputFile, token, 600 * 10/*調査上限*/, fileLoadPercent);
                conn.Commit();
            }
            catch (Exception e)
            {
                conn.Rollback();
                d(e.ToString());
                return;
            }
            var macList = getSummaryListByMac(token, 100/*この個数以下の情報は破棄*/);
            if (macList == null) return;

            //MAC毎にK-means法
            var listClusterHistogramEveryMac 
                = new List<(long mac, List<ClusterMacInfo> list, List<float> listCenter)>();
            //foreach (var n in macList)
            Parallel.ForEach(macList, (n) =>
             {
                 if (token.IsCancellationRequested==false)
                 {
                     var list = doKmeans(n, token);
                     d($"mac: {n.mac.ToString("X")}," +
                         " point(s): " +
                         String.Join(", ", list.Select(q => (q / 10).ToString("F1")).ToList()));

                    //出来たクラスタ毎に最小、最大、標準偏差を求める。
                    var listClusterMacInfo = calcClusterMinMaxStdDev(n.mac, list, token);
                     listClusterHistogramEveryMac.Add((mac: n.mac, list: listClusterMacInfo, listCenter: list));
                     //break;//デバッグ時、1つのみのMACを対象とする場合に有効化
                 }
             });
            d("K-means done.");

            d("結果の出力");
            fileExport(listClusterHistogramEveryMac);
            d("出力完了");

        }

        private void fileExport(List<(long mac, List<ClusterMacInfo> list, List<float> listCenter)> listClusterHistogramEveryMac)
        {
            //foreach (var item in listClusterHistogramEveryMac)
            Parallel.ForEach(listClusterHistogramEveryMac, (item) =>
            {
                d($"---{item.mac.ToString("X")}の処理開始---");
                var totalCnt = item.list.Select(q => (long)q.macInfo.count).Sum();
                var ticks = conn.Table<CsvRow>()
                                    .Where(q => q.mac == item.mac)
                                    .Min(q => q.ticks);
                var startDt = new DateTime(ticks);
                ticks = conn.Table<CsvRow>()
                                    .Where(q => q.mac == item.mac)
                                    .Max(q => q.ticks);
                var endtDt = new DateTime(ticks);

                //80%内の平均CT算出
                var total80Cnt = (long)(totalCnt * 0.8d);
                d("80%リストの作成 ", false);
                var ave80CtList = conn.Table<CsvRow>()
                                .Where(q => q.mac == item.mac)
                                .Select(q => q.ct / 10.0)
                                .OrderBy(q => q)
                                .Take((int)total80Cnt);
                d("完了、80%平均値の作成 ", false);
                var ave80Ct = ave80CtList.Average();
                d("完了、80%中央値の作成", false);
                var ave80median = Statistics.Median(ave80CtList);

                d("完了、ファイル出力", false);
                var outFile = $"{totalCnt.ToString("D8")}_{item.mac.ToString("X")}_{ave80Ct.ToString("F1")}.csv";
                if (File.Exists(outFile)) { File.Delete(outFile); }
                var enc = Encoding.GetEncoding("shift_jis");
                using (var sw = new StreamWriter(outFile, false, enc))
                {
                    sw.WriteLine(item.mac.ToString("x"));
                    sw.WriteLine($"集計対象期間: {startDt.ToString("yyyy/MM/dd HH:mm")}～" +
                        $"{endtDt.ToString("yyyy/MM/dd HH:mm")}");
                    sw.WriteLine();
                    sw.WriteLine("全回数,全秒数");
                    var totalSec = conn.Table<CsvRow>().Where(q => q.mac == item.mac).Select(q => q.ct / 10.0).Sum();
                    sw.WriteLine($"{totalCnt},{totalSec.ToString("F1")}");
                    sw.WriteLine();
                    sw.WriteLine("全体の上位80%を対象にした平均値・中央値");
                    sw.WriteLine("平均値,中央値");
                    sw.WriteLine($"{ave80Ct},{ave80median}");
                    sw.WriteLine();
                    sw.WriteLine("クラスタ");
                    sw.WriteLine("Cluster#,from(s),to(s),平均,回数,標準偏差,合計秒");
                    foreach (var c in item.list)
                    {
                        var center = item.listCenter[(int)c.macInfo.mac];
                        sw.WriteLine(
                            $"{(int)c.macInfo.mac}," +
                            $"{(c.macInfo.min / 10.0).ToString("F1")}," +
                            $"{(c.macInfo.max / 10.0).ToString("F1")}," +
                            $"{(center / 10.0).ToString("F1")}," +
                            $"{c.macInfo.count}," +
                            $"{c.macInfo.stdev.ToString("F1")}," +
                            $"{((center / 10.0) * c.macInfo.count).ToString("F1")}");
                    }
                    sw.WriteLine("");
                    sw.WriteLine("ヒストグラム");
                    sw.WriteLine("Cluster#,from(s),to(s),回数,平均,積算秒/単位秒,最終信号");
                    foreach (var c in item.list)
                    {
                        foreach (var h in c.listHistogramInfo)
                        {
                            sw.WriteLine($"{c.macInfo.mac}," +
                                $"{h.rangeFrom.ToString("F1")}," +
                                $"{h.rangeTo.ToString("F1")}," +
                                $"{h.count},{(h.avg).ToString("F1")}," +
                                $"{h.totalSecPerUnitTime.ToString("F1")}," +
                                $"{h.lastDateTimeStr}");
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                }
                d("");
            });
        }

        private class ClusterMacInfo
        {
            public MacInfo macInfo = null;
            public HistogramInfo histogramInfo = null;
            //public List<(string name, int cnt, int avg)> listHistogramNode = null;
            public List<HistogramInfo> listHistogramInfo = null;
        }
        private class HistogramInfo
        {
            public int clusterNo;
            public float rangeFrom;
            public float rangeTo;
            public int count;
            public float avg;
            //単位時間あたりの消費時間積算値
            public double totalSecPerUnitTime;
            public string lastDateTimeStr;
        }

        private List<ClusterMacInfo> calcClusterMinMaxStdDev(long mac, List<float> kmeanResult, CancellationToken token)
        {
            var listClusterMacInfo = new List<ClusterMacInfo>();

            //並列処理を想定し、独立したメモリDBを確保する。
            using(var co = new SQLiteConnection(":memory:"))
            {
                //クラスタ計算用の暫定テーブル作成
                co.CreateTable<ClusterRow>();

                //計算用テーブルに登録
                co.BeginTransaction();
                var rs = conn.Query<CsvRow>("select ct, ticks from CsvRow" +
                    $" where mac='{mac}'");
                var timerDt = DateTime.Now.AddSeconds(5);
                foreach (var row in rs)
                {
                    var tmpList = new List<(int x, float gap)>();
                    for (int i = 0; i < kmeanResult.Count; i++)
                    {
                        tmpList.Add((x: i, gap: Math.Abs(kmeanResult[i] - row.ct)));
                    }
                    var minGap = tmpList.OrderBy(q => q.gap).First();

                    //誤差最小のクラスタインデックスにて計算用テーブルに登録
                    co.Insert(new ClusterRow() { idx = minGap.x, value = row.ct, ticks = row.ticks });
                    if (DateTime.Now > timerDt)
                    {
                        d("+", false);
                        timerDt = DateTime.Now.AddSeconds(5);
                    }
                    if (token.IsCancellationRequested)
                    {
                        co.Rollback();
                        return null;
                    }
                }
                co.Commit();
                d("+");

                //最小、最大を計算(mac部分をインデックスとして代用していることに注意)
                var rs2 = co.Query<MacInfo>("select idx as mac," +
                    " min(value) as min, max(value) as max, count(value) as count" +
                    " from ClusterRow group by idx");
                var listMacInfo = new List<MacInfo>();
                listMacInfo.AddRange(rs2.ToArray());

                //クラスタ毎の標準偏差を計算
                for (var i = 0; i < kmeanResult.Count; i++)
                {
                    if (token.IsCancellationRequested) { return null; }
                    var row = listMacInfo[i];
                    var valueList = co.Table<ClusterRow>().Where(q => q.idx == i).Select(q => (float)q.value).ToList();

                    var stdev = Statistics.PopulationStandardDeviation(valueList);
                    listMacInfo[i].stdev = stdev;

                    var clusterValue = (kmeanResult[(int)row.mac/*index*/] / 10.0).ToString("F1");
                    /* d($"cluster: {clusterValue} " +
                        $"min: {row.min} max: {row.max}  stdev: {row.stdev.ToString("F1")} count: {row.count}");//*/

                    //ヒストグラムのランクを算出 -4σ～0～4σまでの8区分
                    MacInfo preRow = null;
                    if (i > 0) { preRow = listMacInfo[i - 1]; }
                    var rangeList = calcRank(kmeanResult, i, row, preRow);

                    //ヒストグラム計算
                    //var histResult = new List<(string name, int cnt, int avg)>();
                    var histInfo = new List<HistogramInfo>();
                    foreach (var range in rangeList)
                    {
                        if (token.IsCancellationRequested) { return null; }
                        var query = "select count(value) as value, avg(value) as idx from ClusterRow" +
                            $" where value>={range.min} and value<{range.max}";
                        var result = co.Query<ClusterRow>(query).First();
                        var key = $"{(range.min / 10.0).ToString("F1")},{(range.max / 10.0).ToString("F1")}";
                        //d($"  {key}\t{result.value}\t{(result.idx / 10.0).ToString("F1")}");
                        d(".", false);
                        //histResult.Add((name: key, cnt: result.value, avg: result.idx));
                        var lastDateTimeStr = "";
                        if (result.value > 0)
                        {
                            var lastTicks = co.Table<ClusterRow>()
                                .Where(q => (q.value >= range.min) && (q.value < range.max))
                                .Select(q => q.ticks).Max();
                            lastDateTimeStr = new DateTime(lastTicks).ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        histInfo.Add(new HistogramInfo()
                        {
                            clusterNo = i,
                            rangeFrom = range.min / 10.0f,
                            rangeTo = range.max / 10.0f,
                            count = result.value,
                            avg = result.idx / 10f,
                            totalSecPerUnitTime = ((result.idx / 10.0) * result.value) / ((range.max - range.min) / 10.0),
                            lastDateTimeStr = lastDateTimeStr
                        });

                    }
                    d(".");
                    listClusterMacInfo.Add(new ClusterMacInfo()
                    {
                        macInfo = listMacInfo[i],
                        listHistogramInfo = histInfo
                    });
                }
            }//using co (SQLiteConnection)

            return listClusterMacInfo;
        }

        
        private List<(int min, int max)> calcRank(List<float> kmeanResult, int i, MacInfo row, MacInfo preRow)
        {
            //ヒストグラムのランクを算出 -4σ～0～4σまでの8区分
            List<(int min, int max)> rangeList = new List<(int min, int max)>();
            var step = 1.0;
            if (row.count > 10) { step = 1.0 / 2.0; }
            if (row.count > 100) { step = 1.0 / 4.0; }
            if (row.count > 1000) { step = 1.0 / 6.0; }

            for (var n = -4.0; n < 4.0; n += step)
            {
                var center = kmeanResult[i];
                var th = (int)(n * row.stdev + center + 0.5/*四捨五入*/);
                var thLess = (int)((n - step) * row.stdev + center + 0.5/*四捨五入*/);
                if (rangeList.Count == 0)
                {
                    if (row.min > thLess) { continue; }
                    if (row.min > th) { continue; }
                    rangeList.Add((min: row.min, max: Math.Max(row.min, th)));
                }
                else if (n == 4.0 - step)
                {
                    var tmpMin = Math.Min(thLess, row.max + 1);
                    if (tmpMin >= row.mac) { continue; }

                    rangeList.Add((min: tmpMin,
                        max: Math.Max(thLess, row.max + 1)));//from <= x < to指定のため、+1する。
                }
                else
                {
                    if (Math.Min(thLess, th) < 0) { continue; }
                    if(Math.Max(thLess,th)>=row.max) { continue; }
                    rangeList.Add((min: Math.Min(thLess, th), max: Math.Max(thLess, th)));
                }
                var range = rangeList[rangeList.Count - 1];
                if (range.min == range.max)
                {
                    rangeList.Remove(range);
                }
                //d($"range: {range.min}～{range.max}");
            }
            if (rangeList.Count == 0) { rangeList.Add((min: row.min, max: row.max + 1)); }
            //最後の配列のmaxがrow.maxでない場合、追加する。
            var lastRange = rangeList[rangeList.Count-1];
            if (lastRange.max < row.max + 1)
            {
                rangeList.Add((min: lastRange.max, max: row.max+1));
            }
            //前回の末尾と今回の先頭のギャップ補間
            if ((preRow != null) && (preRow.max + 1 < row.min))
            {
                //
                var tmpRangeList = new List<(int min, int max)>();
                tmpRangeList.Add((min: preRow.max + 1, max: row.min));
                tmpRangeList.AddRange(rangeList);
                return tmpRangeList;
            }

            return rangeList;
        }

        private List<float> doKmeans(MacInfo info, CancellationToken token, int cluster = 4)
        {
            if (token.IsCancellationRequested) return null;

            //初期値決定
            var ansList = new List<float>();
            var gap = (info.max - info.min) / (cluster - 1);
            for(int i=0; i<cluster; i++)
            {
                ansList.Add(info.min + gap * i);
            }

            //コスト計算ループ
            for(int i=0; i<50; i++)
            {
                if (token.IsCancellationRequested) return null;
                d("o",false);
                var tmpList = calcKmeans(info, ansList);

                //変化有無
                float cost = 0;
                for(var j=0; j<tmpList.Count; j++)
                {
                    cost += Math.Abs(tmpList[j] - ansList[j]);
                    ansList[j] = tmpList[j];
                }
                //d($"i:{i} cost:{cost.ToString("F1")} ary: {String.Join(", ", ansList)}");
                if(cost == 0.0f) { break; }
            }
            d("o");

            ansList.Sort();
            return ansList;
        }
        class AveIfno {
            public int n;
            public float ave;
        }

        private List<float> calcKmeans(MacInfo info, IReadOnlyList<float> rList)
        {
            //--各要素毎の最寄りノード一覧を取得 --
            var rs = conn.Query<CsvRow>($"select ct from CsvRow where mac={info.mac}");

            var aveList = new List<AveIfno>();
            foreach(var n in rList) { aveList.Add(new AveIfno() { n = 0, ave = 0 }); }
            
            foreach(var n in rs)
            {
                var tmpList = new List<(int x, float gap)>();
                for(int i=0; i<rList.Count; i++)
                {
                    tmpList.Add((x: i, gap: Math.Abs(rList[i] - n.ct)));
                }
                var min = tmpList.OrderBy(q => q.gap).First();
                //平均値の更新
                aveList[min.x].n++;
                aveList[min.x].ave =
                    (n.ct + aveList[min.x].ave * (aveList[min.x].n - 1))
                    / aveList[min.x].n;
            }

            //最寄りノード一覧から、新たな重心点を算出
            return aveList.Select(q => q.ave).ToList();
        }

        private List<MacInfo> getSummaryListByMac(CancellationToken token, int countUnderLimit=0)
        {
            if (token.IsCancellationRequested) return null;
            d("mac毎の個数、最大、最小を算出 ・・・", false);
            List<MacInfo> macList = new List<MacInfo>(); 
            d("group by names");
            var rs = conn.Query<MacInfo>("select mac, count, min, max  from " +
                " (select mac, count(mac) as count, min(ct) as min, max(ct) as max" +
                " from CsvRow group by mac) " +
                $" where count>={countUnderLimit} order by count ");
            foreach (var item in rs)
            {
                //d($"{item.mac.ToString("X")} => {item.count}");
                macList.Add(item);
            }
            d($"MACの個数: {rs.Count}");
            return macList;
        }

        private void insertMemory(string file, CancellationToken token, int upperLimitDSec = 3600*10, int percent=100)
        {
            using (var sr = new StreamReader(file))
            {
                d("開始");
                var dt = DateTime.Now;
                d(((sr.BaseStream.Position * 100.0d) / sr.BaseStream.Length).ToString("F1"));
                var limitDt = DateTime.Now.AddSeconds(0.2);
                while (token.IsCancellationRequested == false)
                {
                    var line = sr.ReadLine();
                    if (line == null) break;
                    insertCsvLine(line, upperLimitDSec);
                    if (DateTime.Now > limitDt)
                    {
                        var prog = (sr.BaseStream.Position * 100.0d) / sr.BaseStream.Length;
                        d(prog.ToString("F1"));
                        if (prog > percent) { break; }
                        limitDt = DateTime.Now.AddSeconds(0.2);
                    }
                }
                d(((sr.BaseStream.Position * 100.0d) / sr.BaseStream.Length).ToString("F1"));
                d("終了");
                d("time: " + (DateTime.Now - dt).TotalSeconds.ToString("F1"));
                d("Count: " + conn.Table<CsvRow>().Count());
            }
        }

        public void Dispose()
        {
            if (conn == null) return;
            conn.Dispose();
            conn = null;
        }

        private class MacInfo
        {
            public long mac { get; set; } = 0;
            public int count { get; set; } = 0;
            public int min { get; set; } = 0;
            public int max { get; set; } = 0;
            public double stdev { get; set; } = 0.0d;
        }
        private class CalcCost
        {
            public int center { get; set; } = 0;
            public int ct { get; set; } = 0;
        }
    }
}
