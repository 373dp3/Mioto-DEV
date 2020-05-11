using KmeansTool.Table;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KmeansTool
{
    public class MemDb : GuiCommon
    {
        SQLiteConnection conn;
        public MemDb()
        {
            conn = new SQLiteConnection(":memory:");
        }

        public void insertCsvLine(string csv)
        {
            var row = CsvRow.FromCsv(csv);
            if(row==null) { d("line is null"); return; }
            
        }

        public async void load(string file, CancellationToken token)
        {
            await Task.Run(() =>
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
                        insertCsvLine(line);
                        if (DateTime.Now > limitDt)
                        {
                            d(((sr.BaseStream.Position * 100.0d) / sr.BaseStream.Length).ToString("F1"));
                            limitDt = DateTime.Now.AddSeconds(0.2);
                        }
                    }
                    d(((sr.BaseStream.Position * 100.0d) / sr.BaseStream.Length).ToString("F1"));
                    d("終了");
                    d("time: " + (DateTime.Now - dt).TotalSeconds.ToString("F1"));
                }
            });
        }
    }
}
