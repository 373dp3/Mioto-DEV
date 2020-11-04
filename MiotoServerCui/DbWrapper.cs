/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using MiotoBlazorCommon.DB;
using MiotoBlazorCommon.Struct;
using MiotoServer.DB;
using MiotoServer.Query;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class DbWrapper
    {
        private static DbWrapper instance = null;
        public ulong beginRowId { get; private set; }
        public static DbWrapper getInstance(string dbPath = null)
        {
            if (instance != null) { return instance; }
            if (dbPath == null) { throw new ArgumentException("初回呼び出し時は必ずNULL以外を指定してください"); }
            instance = new DbWrapper(dbPath);
            return instance;
        }

        public SQLiteConnection conn = null;
        public SQLiteConnection memConn = null;

        const string dbFilePrefix = "mioto_db";

        /**
         * 目的
         *  latestinfo: 重複チェック用テーブル
         *  mac、seq、btn, ticks
         *  
         *  csvcash: CSV出力キャッシュテーブル
         *  id, mac, dt, csvstring
         * */
        private DbWrapper(string dbDirPath)
        {
            //メモリDBを対象に処理する
            var dbfiledt = DateTime.Now;

            //末尾のスラッシュを調整
            dbDirPath = dbDirPath.Replace('\\', '/');
            if (dbDirPath[dbDirPath.Length-1] != '/')
            {
                dbDirPath += "/";
            }
            //[TODO]sqlite-net移植時に無効化。最有効化が必要か？ doMigrateIfNeed(dbfiledt, dbDirPath);

            var dbPath = dbDirPath + dbFilePrefix + "_v0.4.db";
            var isDbExist = File.Exists(dbPath);
            if (Directory.Exists(dbDirPath) == false)
            {
                Directory.CreateDirectory(dbDirPath);
            }
            conn = new SQLiteConnection(dbPath);
            memConn = new SQLiteConnection(":memory:");


            //テーブル作成
            conn.CreateTable<csvcash>();
            conn.CreateTable<date2row>();
            conn.CreateTable<LastInfo>();
            conn.CreateTable<latest2525>();
            conn.CreateTable<ConfigTbl>();
            conn.CreateTable<ProductionFactor>();
            memConn.CreateTable<MacTicks>();
            memConn.CreateTable<ConnCounter>();

            //DBフィールドを確認し必要であれば移行する。
            //migrateDbIfNeed();

            //URL無指定時に表示するデータの起点
            beginRowId = getMaxDate2RowId();

        }

        private void doMigrateIfNeed(DateTime dt, string dbDirPath)
        {
            throw new NotImplementedException();
            /*

            var db02 = dbDirPath + "2019_v0.2.db";
            var db03 = dbDirPath + "2019_v0.3.db";
            var db04 = dbDirPath + dbFilePrefix + "_v0.4.db";
            //0.2->0.3
            if (File.Exists(db02) && (File.Exists(db03)==false))
            {
                File.Copy(db02, db03);

                var sqlConnectionSb = new SQLiteConnectionStringBuilder
                {
                    DataSource = db03,
                    Version = 3,
                    LegacyFormat = false,
                    SyncMode = SynchronizationModes.Normal,
                    JournalMode = SQLiteJournalModeEnum.Memory
                };
                using (var connTmp = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    connTmp.Open();
                    using (var cmd = connTmp.CreateCommand())
                    {
                        cmd.CommandText = "alter table csvcash add column [flg] INTEGER;";
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = connTmp.CreateCommand())
                    {
                        cmd.CommandText = "CREATE INDEX [csvcashMacIdx] ON [csvcash] (mac);";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            //0.3->0.4
            if (File.Exists(db03) && (File.Exists(db04) == false))
            {
                File.Copy(db03, db04);

                var sqlConnectionSb = new SQLiteConnectionStringBuilder
                {
                    DataSource = db04,
                    Version = 3,
                    LegacyFormat = false,
                    SyncMode = SynchronizationModes.Normal,
                    JournalMode = SQLiteJournalModeEnum.Memory
                };
                using (var connTmp = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    
                    connTmp.Open();
                    using (var cmd = connTmp.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE [latest2525] ([mac] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, [state] INTEGER, PRIMARY KEY(mac));";
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = connTmp.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE [csvcash2] ([mac] INTEGER, [ticks] INTEGER, [csv] TEXT, [flg] INTEGER);";
                        cmd.ExecuteNonQuery();
                    }
                    
                    using (var readCmd  = connTmp.CreateCommand())
                    using (var insCmd   = connTmp.CreateCommand())
                    {
                        var tr = connTmp.BeginTransaction();
                        readCmd.CommandText = "select mac, ticks, csv, flg from csvcash";
                        using (var reader = readCmd.ExecuteReader())
                        {
                            for (var i = 0; reader.Read(); i++)
                            {
                                var n = reader[3].ToString();
                                if(n.Length==0) { n = "NULL"; }
                                var csv = "2019/"+reader[2].ToString();
                                csv = Regex.Replace(csv, ",*$", "");
                                var query = $"INSERT INTO csvcash2 (mac, ticks, csv, flg) values ({reader[0]}, {reader[1]}, '{csv}', {n})";
                                insCmd.CommandText = query;
                                insCmd.ExecuteNonQuery();
                            }
                        }
                        tr.Commit();
                    }
                    using (var cmd = connTmp.CreateCommand())
                    {
                        cmd.CommandText = "DROP TABLE csvcash;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "ALTER TABLE csvcash2 RENAME TO csvcash;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE INDEX [csvcashMacIdx] ON [csvcash] (mac);";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "vacuum;";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            //*/

        }

        WebSocketMgr sockMgr = WebSocketMgr.getInstance();
        public void insertCsv(TweCtPacket packet, string csv)
        {
            var data = csv;
            string query = "INSERT INTO csvcash (mac, ticks, csv) "
                + " values ("
                + packet.mac + ", "
                + packet.dt.Ticks + ", '" + data + "')";
            conn.Execute(query);
            updateDateFlg();

        }

        const int FLG_TWE = 2;
        const int FLG_PAL = 3;
        const int FLG_2525 = 4;
        const int FLG_AMPERE = 5;
        public const string CONFIG_BLAZOR_KEY = "BLAZOR_CONFIG";

        public void insertCsv(TwePacket packet, string csv)
        {
            var data = csv;
            string query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                + " values ("
                + packet.mac + ", "
                + packet.dt.Ticks + ", '" + data + "', "+ FLG_TWE+")";//純正TWE-Liteパケットは2固定
            conn.Execute(query);
            updateDateFlg();
        }

        public void insertCsv(TwePalSensePacket pal)
        {
            var query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                        + " values ("
                        + pal.mac + ", "
                        + pal.dt.Ticks + ", '" + pal.toCsv() + "', " + FLG_PAL + ")";//TWE-Lite PALパケットは3固定
            conn.Execute(query);
            updateDateFlg();
        }

        public void insertCsv(Mac2Ampere amp)
        {
            var query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                        + " values ("
                        + amp.mac + ", "
                        + amp.ticks + ", '" + Mac2Ampere.ToCSV(amp) + "', " + FLG_AMPERE + ")";//Ah(アンペアアワー)
            conn.Execute(query);
            updateDateFlg();
        }

        public async Task<string> getCsv(Param param)
        {
            switch(param.type)
            {
                case Param.TYPE.BLAZOR_CLIENT_POLLING:
                    return await GetMixedCsvAsync(param);
                case Param.TYPE.BLAZOR_CONFIG:
                    return GetBlazorConfig();
                case Param.TYPE.MAC2MACHINE:
                    return GetMac2Machine();
                default:
                    return _getCsv(param);
            }
        }

        public string GetMac2Machine()
        {
            var jsonCfg = getConfig(CONFIG_BLAZOR_KEY);
            var cfg = JsonSerializer.Deserialize<Config>(jsonCfg);
            var sb = new StringBuilder();
            foreach(var twe in cfg.listTwe)
            {
                sb.AppendLine($"{twe.mac.ToString("x")},{twe.name}");
            }

            return sb.ToString();
        }

        public string GetBlazorConfig()
        {
            var cfg = new Config();
            try
            {
                var jsonCfg = getConfig(CONFIG_BLAZOR_KEY);
                cfg = JsonSerializer.Deserialize<Config>(jsonCfg);
                
                var lastInfoList = conn.Query<LastInfo>("select * from LastInfo group by mac").ToList();
                foreach(var twe in cfg.listTwe)
                {
                    var info = lastInfoList.Where(q=>q.mac == twe.mac).FirstOrDefault();
                    if (info == null) { continue; }
                    if (info.ticks==0) { continue; }
                    twe.Ticks = info.ticks;
                }
            }
            catch (Exception e)
            {
                cfg.insertOrUpdateTwe(getLastInfoList());
            }

            //TWE-Lite子機のリスト確認と追加
            var list = DbWrapper.getInstance().getLastInfoList();
            foreach (var twe in list)
            {
                if (cfg.listTwe.Where(q => q.mac == twe.mac).Count() != 0) continue;
                cfg.listTwe.Add(new ConfigTwe() { mac = twe.mac, Ticks = twe.ticks });
            }

            cfg.appVer = MiotoServerWrapper.config.appVer;
            var json = JsonSerializer.Serialize(cfg);
            return json;
        }

        public async Task<string> GetMixedCsvAsync(Param param)
        {
            //CSV情報取得
            await Task.Yield();
            var sb = new StringBuilder();
            if(await pollingUntilUpdate(param) == false)
            {
                return "";
            }
            sb.Append(GetProductionFactorCsv(param, true));//数は大した事がないし、更新されている可能性もあるので
            param.option = Param.OPTION.BACKUP;
            sb.Append(_getCsv(param));

            return sb.ToString();
        }

        public string _getCsv(Param param)
        {
            //*
            //string hhmm = Program.config[Program.HM_KEY];
            string hhmm = MiotoServerWrapper.config.hhmm.ToString();
            string header = "";
            string afterHeader = "";
            string whereKey = "";
            bool isHeader = param.option != Param.OPTION.BACKUP;
            switch (param.type)
            {
                case Param.TYPE.CT:
                    header = "date time,mac,seq,btn,batt,lqi,0_00,0_01,0_10,0_11,DI\r\n";
                    whereKey = " flg IS NULL and ";
                    break;
                case Param.TYPE.TWE:
                    header = "date time,mac,btn,btnChg,batt,lqi,ad1,ad2,ad3,ad4\r\n";
                    whereKey = " flg=" + FLG_TWE + " and ";
                    break;
                case Param.TYPE.PAL:
                    header = "date time,mac,lqi,batt,temp,humi,lux\r\n";
                    whereKey = " flg=" + FLG_PAL + " and ";
                    break;
                case Param.TYPE.TWE2525:
                    header = "date time,mac,lqi,batt,stat\r\n";
                    whereKey = " flg=" + FLG_2525 + " and ";
                    break;
                case Param.TYPE.AH:
                    header = "date time,mac,Ah1,Ah2,Ah3,Ah4,Ah5,maxA1,maxA2,maxA3,maxA4,maxA5\r\n";
                    whereKey = " flg=" + FLG_AMPERE + " and ";
                    var ampList = DbComSerial.getInstance().getMac2AmpereList();
                    afterHeader = string.Join("\r\n", ampList.ToArray()) + "\r\n";
                    //FIXnn 指定がある場合、総数を調整する。
                    if(param.fixRow != Param.FIX_ROW_NOOP)
                    {
                        param.fixRow -= ampList.Count;
                    }
                    break;
            }
            if (!isHeader) { header = ""; }

            StringBuilder sb = new StringBuilder();
            sb.Append(header);
            ulong from = beginRowId;
            ulong to = ulong.MaxValue;

            var startDt = DbChecker.getInstance().getDateTimeOfWorkDay(param.dateList);
            var endDt = DbChecker.getInstance().getDateTimeOfWorkDay(param.dateList, false);

            //日付指定がある場合は、その範囲でfrom、toを上書き
            if (param.dateList.Count > 0)
            {
                getCsvTweFromTo(param.dateList, ref from, ref to);

                //値が更新されていない場合は該当する日付の情報が存在しないため、
                //処理を終了する。
                if (to == ulong.MaxValue)
                {
                    if (isHeader) sb.Append(endDt.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n");
                    if (isHeader) sb.Append(startDt.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n");
                    return sb.ToString();
                }

                //値が更新されていない場合は該当する日付の情報が存在しないため、
                //処理を終了する。
                if (to == ulong.MaxValue)
                {
                    if (isHeader) sb.Append(endDt.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n");
                    if (isHeader) sb.Append(startDt.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n");
                    return sb.ToString();
                }
            }
            if (isHeader) sb.Append(endDt.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n");

            //CSV情報取得
            var macListStr = "";
            if (param.macList.Count > 0)
            {
                macListStr = " and ( mac = " + string.Join(" or mac = ", param.macList) + " ) ";
            }
            var fixRow = " ";
            if (param.fixRow != Param.FIX_ROW_NOOP)
            {
                fixRow = " limit " + param.fixRow;
            }

            //出力量の制御
            var CommandText = "";
            switch (param.volume)
            {
                case Param.VOLUME.NORMAL:
                    //間引かない
                    CommandText = "select ticks, csv from csvcash where " + whereKey
                        + " ROWID > " + from + " and ROWID <=" + to + macListStr
                        + " and ticks > " + param.orderMinTicks
                        + " order by ticks DESC " + fixRow;
                    if (param.option == Param.OPTION.BACKUP)
                    {
                        CommandText = "select ticks, csv from csvcash where " + whereKey
                            + " ROWID > " + from + " and ROWID <=" + to + macListStr
                        + " and ticks > " + param.orderMinTicks
                            + " order by ticks ASC " + fixRow;
                    }
                    break;
                case Param.VOLUME.THINING:
                    UInt64 val = (UInt64)param.thiningSec * 10000000UL;
                    //一定時間間隔で間引く
                    CommandText = "select ticks, csv, cast(ticks/(" + val + ") as int) as tick2 "
                        + "from csvcash where " + whereKey
                        + " ROWID > " + from + " and ROWID <=" + to + macListStr
                        + " group by mac, tick2 "
                        + " order by ticks DESC "
                        + " " + fixRow;
                    break;
                case Param.VOLUME.FINAL:
                    if (param.type == Param.TYPE.TWE2525)
                    {
                        //[latest2525] ([mac] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, [state] INTEGER, PRIMARY KEY(mac))
                        //最終データのみを対象とする
                        CommandText = "select mac, lqi, batt, state, ticks from latest2525 ";
                        if (macListStr.Length > 0)
                        {
                            CommandText += " where " + macListStr;
                        }
                    }
                    else
                    {
                        //最終データのみを対象とする
                        CommandText = "select ticks, csv, max(ticks) from csvcash where " + whereKey
                            + " ROWID > " + from + " and ROWID <=" + to + macListStr
                            + " group by mac order by ticks DESC ";
                    }
                    break;
            }
            if ((param.volume == Param.VOLUME.FINAL) && (param.type == Param.TYPE.TWE2525))
            {
                var rs = conn.Query<latest2525>(CommandText);
                long i = 0;
                foreach (var item in rs)
                {
                    sb.Append(new DateTime((long)item.ticks).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Append(String.Format(",{0:x},{1:d},{2:F1},{3:d}",
                        item.mac, item.lqi, Convert.ToInt32(item.batt) / 10.0, item.state) + "\r\n");
                    i++;
                }
                if (param.fixRow != Param.FIX_ROW_NOOP)
                {
                    for (; i < param.fixRow; i++)
                    {
                        sb.AppendLine();
                    }
                }
                param.ansMaxTicks = rs.Select(q => q.ticks).Max();
            }
            else
            {
                sb.Append(afterHeader);
                var rs = conn.Query<csvcash>(CommandText);
                long i = 0;
                foreach(var item in rs)
                {
                    sb.Append(item.csv + "\r\n");
                    i++;
                }
                if (param.fixRow != Param.FIX_ROW_NOOP)
                {
                    for (; i < param.fixRow; i++)
                    {
                        sb.AppendLine();
                    }
                }
                if (rs.Count > 0)
                {
                    param.ansMaxTicks = rs.Select(q => q.ticks).Max();
                }

            }
            if (isHeader) sb.Append(startDt.ToString("yyyy/MM/dd HH:mm:ss") + "\r\n");
            return sb.ToString();
            //*/
        }

        private void getCsvTweFromTo(List<UInt32> ymdList, ref ulong from, ref ulong to)
        {
            var CommandText = "";
            if (ymdList.Count == 1)
            {
                CommandText = "select startrow, endrow from date2row where date = " + ymdList[0];
            }
            else
            {
                CommandText = "select startrow, endrow from date2row where date >= "
                    + ymdList[0] + " and date <= " + ymdList[1] + " order by date";
            }
            long i = 0;
            var rs = conn.Query<date2row>(CommandText);
            foreach(var item in rs)
            {
                if (item.startrow.ToString().Length == 0) continue;
                if (i == 0) { from = Convert.ToUInt64(item.startrow); }
                to = Convert.ToUInt64(item.endrow);
                i++;
            }

            //エンド指定が今日もしくはそれよりも未来であればtoを無指定に
            if ((ymdList.Count <= 2)
                && (ymdList[ymdList.Count-1] >= Convert.ToUInt32(DateTime.Now.ToString("yyyyMMdd"))))
            {
                to = long.MaxValue - 1;//MaxValueは検索失敗を示す無効値として使用しているの為、-1
            }


        }

        private ulong getMaxRowId()
        {
            /*
            ulong rowid = 0;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "select max(ROWID) from csvcash";
                using (var reader = readCmd.ExecuteReader())
                {
                    for (var i = 0; reader.Read(); i++)
                    {
                        if (reader[0].ToString().Length == 0) continue;
                        rowid = Convert.ToUInt64(reader[0]);
                        break;
                    }
                }
            }
            //*/


            return (ulong)conn.ExecuteScalar<long>("select max(ROWID) from csvcash");
        }
        private ulong getMaxDate2RowId()
        {
            try
            {
                return (ulong)conn.Table<date2row>().Max(v => v.endrow);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public long getMaxTick()
        {
            try
            {
                var ans = conn.Query<csvcash>("SELECT * from csvcash order by ROWID desc limit 1");
                if (ans == null) { return 0; }
                if (ans.Count == 0) { return 0; }
                return ans[0].ticks;
            }
            catch
            {
                return 0;
            }
        }
        public bool isDate2RowExist(string dateStr)
        {
            try
            {
                var list = conn.Table<date2row>().ToList();
                var dt = Convert.ToInt32(dateStr);
                var listId = list.LastOrDefault(q => q.date == dt);
                if(listId == null) { return false; }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void updateDate2Row(string dateStr)
        {
            //重複回避のため、対象となる日付が登録済みかを確認する。
            if (isDate2RowExist(dateStr)) { return; }

            //csvcashの最大ROWID取得
            ulong rowid = getMaxRowId();

            //date2rowのendrowを取得
            ulong rowidCashMax = getMaxDate2RowId();

            Program.d(dateStr + ", " + rowidCashMax + ", " + rowid);
            
            //"CREATE TABLE [date2row] ([date] INTEGER, [startrow] INTEGER, [endrow] INTEGER);"
            string query = "INSERT INTO date2row (date, startrow, endrow) "
                + " values ("
                + dateStr + ", "
                + rowidCashMax + ", "
                + rowid + ")";

            conn.Execute(query);
            updateDateFlg();

            refreshLastInfoList();//mac, btnにてGroup化したリストのみを残す。

            conn.Execute("vacuum;");

            beginRowId = rowid;

            //メモリDBのリフレッシュ
            memConn.BeginTransaction();
            memConn.DropTable<MacTicks>();
            memConn.CreateTable<MacTicks>();
            memConn.Commit();
        }

        public List<TweCtPacket> getLatestTweCtPacketByMac(UInt32 mac)
        {
            //seq, btn, lqi, batt, ticks
            var CommandText = "select * from (select * from LastInfo where mac=" + mac + " order by ticks DESC) group by btn";
            var rs = conn.Query<LastInfo>(CommandText);
            List<TweCtPacket> ary = new List<TweCtPacket>();
            foreach(var item in rs)
            {
                ary.Add(new TweCtPacket((uint)item.mac, (byte)item.seq, (byte)item.btn, (byte)item.lqi, (ushort)item.batt, item.ticks));
            }
            return ary.OrderByDescending(q=>q.dt.Ticks).ToList();
        }

        public Twe2525APacket getLatestTwe2525PacketByMac(UInt32 mac)
        {
            Twe2525APacket packet = null;
            //*
            var avoidUrlList = new List<string>();
            //[latest2525] ([mac] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, [state] INTEGER
            var CommandText = "select * from latest2525 where mac=" + mac;
            try
            {
                var ans =  conn.Table<latest2525>().Where(v => v.mac == mac).FirstOrDefault();
                return new Twe2525APacket((uint)ans.mac, (int)ans.lqi, (int)ans.batt, ans.ticks, (int)ans.state);
            }catch(Exception e)
            {
                return null;
            }
        }

        public void insertOrUpdatePacket(TweCtPacket packet)
        {
            var lastInfo = new LastInfo {
                mac = packet.mac, seq = packet.seq, btn = packet.btn,
                lqi = packet.lqi, batt = packet.batt, ticks = packet.dt.Ticks };
            conn.Insert(lastInfo);
            updateMacTicks(packet.mac, packet.dt.Ticks);
            updateDateFlg();
        }

        public void insertOrUpdatePacket(Twe2525APacket packet)
        {
            conn.BeginTransaction();
            {
                string query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                        + " values ("
                        + packet.mac + ", "
                        + packet.dt.Ticks + ", '" + packet.toCsv() + "', " + FLG_2525 + ")";
                conn.Execute(query);
            }
            {
                string query = "INSERT OR REPLACE INTO latest2525 (mac, lqi, batt, ticks, state) "
                        + " values ("
                        + packet.mac + ", "
                        + packet.lqi + ", "
                        + packet.batt * 10 + ", "
                        + packet.dt.Ticks + ", "
                        + packet.state
                        + ")";
                conn.Execute(query);
                updateDateFlg();
            }
            updateMacTicks(packet.mac, packet.dt.Ticks);
            conn.Commit();
        }

        string lastUpdate = DateTime.Now.ToUniversalTime().ToString("r");
        public void updateDateFlg()
        {
            lastUpdate = DateTime.Now.ToUniversalTime().ToString("r");
        }
        public string getLastUpdateHttpdFormat()
        {
            return lastUpdate;
        }
        public bool isModified(string httpDateString)
        {
            return lastUpdate.CompareTo(httpDateString) != 0;
        }
        public void Dispose()
        {

        }

        public List<LastInfo> getLastInfoList()
        {
            return conn.Query<LastInfo>("select * from LastInfo group by mac order by ticks DESC").ToList();
        }

        public void refreshLastInfoList()
        {
            var list = conn.Query<LastInfo>("select * from LastInfo group by mac, btn order by ticks DESC").ToList();
            conn.DropTable<LastInfo>();
            conn.CreateTable<LastInfo>();
            conn.InsertAll(list);
        }

        public string getConfig(string key)
        {
            try
            {
                //[注意] SQLite-netにおける文字列比較はCompareToではなく==にて
                return conn.Table<ConfigTbl>().Where(q => q.key == key).Select(q => q.json).First();
            }catch(Exception e)
            {
                return "";
            }
        }
        public void setConfig(string key, string json)
        {
            var cfg = new ConfigTbl() { key = key, json = json };
            conn.InsertOrReplace(cfg);
        }

        public void d(string msg="")
        {
            Program.d(msg);
        }

        public string GetProductionFactorCsv(Param param, bool isAppendPrefix = false)
        {
            //日付による生産要因情報フィルタ処理の実装
            var dtList = new List<DateTime>();
            if (param.dateList.Count>0)
            {
                dtList.AddRange(param.GetDateTimes());
            }
            else
            {
                dtList.Add(param.GetToday_HHMM());
            }
            var from = dtList.Min();
            var to = dtList.Max().AddDays(1);
            //d($"min:{from.ToString("MM/dd HH:mm")} max:{to.ToString("MM/dd HH:mm")}");

            //基本的なクエリ
            var sb = new StringBuilder();
            var query = $"select * from ProductionFactor Where (stTicks>={from.Ticks} and stTicks<{to.Ticks}) ";

            //macアドレスによるフィルタの追加
            var whereOpt = " ";
            if((param.macList!=null) && (param.macList.Count > 0))
            {
                whereOpt = " and ( mac = " + string.Join(" or mac = ", param.macList) + " ) ";
            }
            query += whereOpt + " order by stTicks ";

            //d(query);
            if (isAppendPrefix)
            {
                foreach (var factor in conn.Query<ProductionFactor>(query))
                {
                    sb.AppendLine("!"+ProductionFactor.KEY+","+factor.ToCSV());
                    param.ansMaxTicks = factor.stTicks;
                }
            }
            else
            {
                foreach (var factor in conn.Query<ProductionFactor>(query))
                {
                    sb.AppendLine(factor.ToCSV());
                    param.ansMaxTicks = factor.stTicks;
                }
            }



            return sb.ToString();
        }

        public void updateMacTicks(long mac, long ticks)
        {
            var cnt = memConn.Table<MacTicks>().Where(q => q.mac == mac).Where(q=> q.ticks > ticks).Count();
            if (cnt > 0)
            {
                //ProductionFactor等で更新されたことを想定し
                //古い値で上書きされることを防ぎ、システムTicksを用いる
                ticks = DateTime.Now.Ticks;
            }
            memConn.InsertOrReplace(new MacTicks { mac = mac, ticks = ticks });
        }

        public async Task<bool> pollingUntilUpdate(Param param, int durationSec = 60, int pollingIntervalMs = 1000)
        {
            var list = param.macList.Select(q => (long)q).ToList();
            var dtLimit = DateTime.Now.AddSeconds(durationSec);
            var query = $"select * from MacTicks where ticks > {param.orderMinTicks} ";
            if (list.Count > 0)
            {
                query += " and ( mac = " + string.Join(" or mac = ", param.macList) + " ) ";
            }
            var ticks = DateTime.Now.Ticks;

            var connCounter = new ConnCounter() { ticks = DateTime.Now.Ticks };
            memConn.InsertOrReplace(connCounter);

            while (DateTime.Now < dtLimit)
            {
                try
                {
                    var ansList = memConn.Query<MacTicks>(query);
                    if ((ansList != null) && (ansList.Count() > 0))
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    d("Exception:" + e.ToString());
                    d(e.StackTrace);
                }
                await Task.Delay(pollingIntervalMs);
                if (param.context != null)
                {
                    var count = memConn.Table<ConnCounter>().Count();
                    if ((count > 5) 
                        && (memConn.Table<ConnCounter>()
                        .Where(q => q.ticks < connCounter.ticks)
                        .Count() == 0))
                    {
                        //接続数が許容量を超えたため古いコネクションからLongPollingを中断する
                        break;
                    }
                }
            }
            memConn.Delete(connCounter);

            return false;
        }

    }
}
