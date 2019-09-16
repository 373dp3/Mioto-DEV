/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using MiotoServer.Query;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
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

        SQLiteConnection conn = null;

        string[] createQueryAry = {
                //(UInt32 mac, byte seq, byte btn, byte lqi, UInt16 batt, long ticks)
                "CREATE TABLE [latestinfo] ([mac] INTEGER, [seq] INTEGER, [btn] INTEGER, [lqi] INTEGER, [batt] INTEGER, [ticks] INTEGER, PRIMARY KEY(mac, btn));",
                "CREATE TABLE [csvcash] ([mac] INTEGER, [ticks] INTEGER, [csv] TEXT, [flg] INTEGER);",
                "CREATE TABLE [date2row] ([date] INTEGER, [startrow] INTEGER, [endrow] INTEGER);",
                "CREATE INDEX [csvcashMacIdx] ON [csvcash] (mac);",
                "CREATE TABLE [cttable] ([ticks] INTEGER, [time] TEXT, [mac] INTEGER, [seq] INTEGER, [btn] INTEGER, [batt] INTEGER, [lqi] INTEGER, [c0_00] REAL, [c0_01] REAL, [c0_10] REAL, [c0_11] REAL, [c1_00] REAL, [c1_01] REAL, [c1_10] REAL, [c1_11] REAL, [c2_00] REAL, [c2_01] REAL, [c2_10] REAL, [c2_11] REAL, [c3_00] REAL, [c3_01] REAL, [c3_10] REAL, [c3_11] REAL, [c4_00] REAL, [c4_01] REAL, [c4_10] REAL, [c4_11] REAL, [c5_00] REAL, [c5_01] REAL, [c5_10] REAL, [c5_11] REAL, [c6_00] REAL, [c6_01] REAL, [c6_10] REAL, [c6_11] REAL, [c7_00] REAL, [c7_01] REAL, [c7_10] REAL, [c7_11] REAL); ",
                "CREATE INDEX [ctMacIdx] ON [cttable] (mac);"
            };

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
            doMigrateIfNeed(dbfiledt);

            //末尾のスラッシュを調整
            dbDirPath = dbDirPath.Replace('\\', '/');
            if (dbDirPath[dbDirPath.Length-1] != '/')
            {
                dbDirPath += "/";
            }
            var dbPath = dbDirPath + dbfiledt.ToString("yyyy") + "_v0.3.db";
            var isDbExist = System.IO.File.Exists(dbPath);
            if (Directory.Exists(dbDirPath) == false)
            {
                Directory.CreateDirectory(dbDirPath);
            }
            var sqlConnectionSb = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3,
                LegacyFormat = false,
                SyncMode = SynchronizationModes.Normal,
                JournalMode = SQLiteJournalModeEnum.Memory
            };
            conn = new SQLiteConnection(sqlConnectionSb.ToString());
            conn.Open();


            //テーブル作成

            if (isDbExist == false)
            {
                using (var tran = conn.BeginTransaction())
                {
                    foreach (var query in createQueryAry)
                    {
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();

                    }
                    tran.Commit();
                    _updateDate();
                }
            }

            //DBフィールドを確認し必要であれば移行する。
            //migrateDbIfNeed();

            //URL無指定時に表示するデータの起点
            beginRowId = getMaxDate2RowId();

        }

        private void doMigrateIfNeed(DateTime dt)
        {
            var db02 = "./db/" + dt.ToString("yyyy") + "_v0.2.db";
            var db03 = "./db/" + dt.ToString("yyyy") + "_v0.3.db";
            var db04 = "./db/" + dt.ToString("yyyy") + "_v0.4.db";
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
            /*
            //0.3->0.4
            if (File.Exists(db03) && (File.Exists(db04) == false))
            {
                File.Copy(db03, db04);

                var sqlConnectionSb3 = new SQLiteConnectionStringBuilder
                {
                    DataSource = db03,
                    Version = 3,
                    LegacyFormat = false,
                    SyncMode = SynchronizationModes.Normal,
                    JournalMode = SQLiteJournalModeEnum.Memory
                };
                var sqlConnectionSb4 = new SQLiteConnectionStringBuilder
                {
                    DataSource = db04,
                    Version = 3,
                    LegacyFormat = false,
                    SyncMode = SynchronizationModes.Normal,
                    JournalMode = SQLiteJournalModeEnum.Memory
                };
                using (var connTmp3 = new SQLiteConnection(sqlConnectionSb3.ToString()))
                using (var connTmp4 = new SQLiteConnection(sqlConnectionSb4.ToString()))
                {
                    connTmp3.Open();
                    connTmp4.Open();

                    //テーブル、インデックス作成
                    foreach (var query in createQueryAry)
                    {
                        if(query.Contains("cttable") == false) { continue; }
                        using (var cmd = connTmp4.CreateCommand())
                        {
                            cmd.CommandText = query;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    //データ抽出
                    using (var readCmd = connTmp3.CreateCommand())
                    {
                        //トランザクション開始
                        using (var cmd4 = connTmp4.CreateCommand())
                        {
                            cmd4.CommandText = "begin;";
                            cmd4.ExecuteNonQuery();
                        }

                        readCmd.CommandText = "select csv, ticks from csvcash where flg IS NULL";

                        using (var reader = readCmd.ExecuteReader())
                        {
                            for (var i = 0; reader.Read(); i++)
                            {
                                var csv = reader[0].ToString().Split(',');
                                if(csv.Length!=38) { continue; }
                                for(var n=0; n<csv.Length; n++)
                                {
                                    if(csv[n].Length==0) { csv[n] = "NULL"; }
                                }
                                csv[0] = "\"" + csv[0] + "\"";
                                csv[1] = Convert.ToUInt64(csv[1], 16).ToString();
                                using (var cmd4 = connTmp4.CreateCommand())
                                {
                                    string query = "INSERT INTO cttable (ticks, time, mac, seq, btn, batt, lqi, c0_00, c0_01, c0_10, c0_11, c1_00, c1_01, c1_10, c1_11, c2_00, c2_01, c2_10, c2_11, c3_00, c3_01, c3_10, c3_11, c4_00, c4_01, c4_10, c4_11, c5_00, c5_01, c5_10, c5_11, c6_00, c6_01, c6_10, c6_11, c7_00, c7_01, c7_10, c7_11) "
                                        + " values ("
                                        + reader[1].ToString() + ", "//ticks
                                        + String.Join(", ", csv)
                                        + ")";
                                    Console.Write(".");
                                    cmd4.CommandText = query;
                                    cmd4.ExecuteNonQuery();
                                }
                            }
                        }

                        //コミット
                        using (var cmd4 = connTmp4.CreateCommand())
                        {
                            cmd4.CommandText = "commit;";
                            cmd4.ExecuteNonQuery();
                        }
                    }
                }
            }
            //*/
        }

        public void insertCsv(TweCtPacket packet, string csv)
        {
            var data = csv;
            string query = "INSERT INTO csvcash (mac, ticks, csv) "
                + " values ("
                + packet.mac + ", "
                + packet.dt.Ticks + ", '" + data + "')";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                _updateDate();
            }
        }

        const int FLG_TWE = 2;
        const int FLG_PAL = 3;


        public void insertCsv(TwePacket packet, string csv)
        {
            var data = csv;
            string query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                + " values ("
                + packet.mac + ", "
                + packet.dt.Ticks + ", '" + data + "', "+ FLG_TWE+")";//純正TWE-Liteパケットは2固定
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                _updateDate();
            }
        }

        public void insertCsv(TwePalSensePacket pal)
        {
            //toCsv
            string query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                + " values ("
                + pal.mac + ", "
                + pal.dt.Ticks + ", '" + pal.toCsv() + "', " + FLG_PAL + ")";//TWE-Lite PALパケットは3固定
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                _updateDate();
            }
            /*
            using (var tran = conn.BeginTransaction())
            {
                foreach(var item in pal.toCsvList())
                {
                    string query = "INSERT INTO csvcash (mac, ticks, csv, flg) "
                        + " values ("
                        + pal.mac + ", "
                        + pal.dt.Ticks + ", '" + item + "', "+ FLG_PAL+")";//TWE-Lite PALパケットは3固定
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }
                }
                _updateDate();
                tran.Commit();
            }
            //*/
        }


        public string getCsv(Param param)
        {
            string hhmm = Program.config[Program.HM_KEY];
            string header = "";
            string whereKey = "";
            switch (param.type)
            {
                case Param.TYPE.CT:
                    header = "date time,mac,seq,btn,batt,lqi,0_00,0_01,0_10,0_11,"
                        + "1_00,1_01,1_10,1_11,2_00,2_01,2_10,2_11,"
                        + "3_00,3_01,3_10,3_11,4_00,4_01,4_10,4_11,"
                        + "5_00,5_01,5_10,5_11,6_00,6_01,6_10,6_11,"
                        + "7_00,7_01,7_10,7_11\r\n";
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
            }

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
                    sb.Append(endDt.ToString("M/dd HH:mm:ss") + "\r\n");
                    sb.Append(startDt.ToString("M/dd HH:mm:ss") + "\r\n");
                    return sb.ToString();
                }

                //値が更新されていない場合は該当する日付の情報が存在しないため、
                //処理を終了する。
                if (to == ulong.MaxValue)
                {
                    sb.Append(endDt.ToString("M/dd HH:mm:ss") + "\r\n");
                    sb.Append(startDt.ToString("M/dd HH:mm:ss") + "\r\n");
                    return sb.ToString();
                }
            }
            sb.Append(endDt.ToString("M/dd HH:mm:ss") + "\r\n");

            //CSV情報取得
            using (var readCmd = conn.CreateCommand())
            {
                var macListStr = "";
                if (param.macList.Count > 0)
                {
                    macListStr = " and ( mac = " + string.Join(" or mac = ", param.macList) + " ) ";
                }
                var fixRow = " ";
                if(param.fixRow != Param.FIX_ROW_NOOP)
                {
                    fixRow = " limit "+param.fixRow;
                }
                
                //出力量の制御
                switch (param.volume)
                {
                    case Param.VOLUME.NORMAL:
                        //間引かない
                        readCmd.CommandText = "select csv from csvcash where " + whereKey
                            + " ROWID > " + from + " and ROWID <=" + to + macListStr
                            + " order by ticks DESC " + fixRow;
                        break;
                    case Param.VOLUME.THINING:
                        UInt64 val = (UInt64)param.thiningSec * 10000000UL;
                        //一定時間間隔で間引く
                        readCmd.CommandText = "select csv, cast(ticks/(" + val + ") as int) as tick2 "
                            + "from csvcash where " + whereKey
                            + " ROWID > " + from + " and ROWID <=" + to + macListStr
                            + " group by mac, tick2 "
                            + " order by ticks DESC "
                            + " " + fixRow;
                        break;
                    case Param.VOLUME.FINAL:
                        //最終データのみを対象とする
                        readCmd.CommandText = "select csv, max(ticks) from csvcash where " + whereKey
                            + " ROWID > " + from + " and ROWID <=" + to + macListStr
                            + " group by mac order by ticks DESC ";
                        break;
                }
                //Program.d(readCmd.CommandText);
                using (var reader = readCmd.ExecuteReader())
                {
                    long i = 0;
                    while (reader.Read())
                    {
                        sb.Append(reader[0] + "\r\n");
                        i++;
                    }
                    //LibreOfficeは日付変更などで行数が一度少なくなると
                    //数式の参照が壊れてしまうため行数を一定にする。
                    if(param.fixRow != Param.FIX_ROW_NOOP)
                    {
                        for (; i < param.fixRow; i++)
                        {
                            sb.AppendLine();
                        }
                    }
                }
            }
            sb.Append(startDt.ToString("M/dd HH:mm:ss") + "\r\n");
            return sb.ToString();
        }


        private void getCsvTweFromTo(List<UInt32> ymdList, ref ulong from, ref ulong to)
        {
            using (var readCmd = conn.CreateCommand())
            {
                if (ymdList.Count == 1)
                {
                    readCmd.CommandText = "select startrow, endrow from date2row where date = " + ymdList[0];
                }
                else
                {
                    readCmd.CommandText = "select startrow, endrow from date2row where date >= "
                        + ymdList[0] + " and date <= " + ymdList[1] + " order by date";
                }
                using (var reader = readCmd.ExecuteReader())
                {
                    for (var i = 0; reader.Read(); i++)
                    {
                        if (reader[0].ToString().Length == 0) continue;
                        if (i == 0) { from = Convert.ToUInt64(reader[0]); }
                        to = Convert.ToUInt64(reader[1]);
                    }
                }
                //エンド指定が今日もしくはそれよりも未来であればtoを無指定に
                if((ymdList.Count==2) 
                    && (ymdList[1] >= Convert.ToUInt32(DateTime.Now.ToString("yyyyMMdd"))))
                {
                    to = long.MaxValue - 1;//MaxValueは検索失敗を示す無効値として使用しているの為、-1
                }
            }
        }


        private ulong getMaxRowId()
        {
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
            return rowid;
        }
        private ulong getMaxDate2RowId()
        {
            ulong rowid = 0;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "select max(endrow) from date2row";
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
            return rowid;
        }

        public long getMaxTick()
        {
            long tick = 0;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "select ticks from csvcash order by ROWID desc limit 1";
                using (var reader = readCmd.ExecuteReader())
                {
                    for (var i = 0; reader.Read(); i++)
                    {
                        if (reader[0].ToString().Length == 0) continue;
                        tick = Convert.ToInt64(reader[0]);
                        break;
                    }
                }
            }
            return tick;
        }
        public bool isDate2RowExist(string dateStr)
        {
            ulong rowid = 0;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "select max(endrow) from date2row " +
                    " where date = " + dateStr;
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
            if (rowid != 0) return true;
            return false;
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
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                _updateDate();
            }
            query = "vacuum;";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
            beginRowId = rowid;

        }

        public List<TweCtPacket> getLatestTweCtPacketByMac(UInt32 mac)
        {
            var readCmd = conn.CreateCommand();
            var avoidUrlList = new List<string>();
            //(UInt32 mac, byte seq, byte btn, byte lqi, UInt16 batt, long ticks)
            readCmd.CommandText = "select seq, btn, lqi, batt, ticks from latestinfo where mac=" + mac +" order by ticks DESC";
            List<TweCtPacket> ary = new List<TweCtPacket>();
            using (var reader = readCmd.ExecuteReader())
            {
                for (var i = 0; reader.Read(); i++)
                {
                    
                    TweCtPacket packet = new TweCtPacket(
                        mac,
                        Convert.ToByte(reader[0]),
                        Convert.ToByte(reader[1]),
                        Convert.ToByte(reader[2]),
                        Convert.ToUInt16(reader[3]),
                        Convert.ToInt64(reader[4]));
                    ary.Add(packet);
                }
            }
            readCmd.Dispose();
            return ary;
        }

        public void insertOrUpdatePacket(TweCtPacket packet)
        {
            string query = "INSERT OR REPLACE INTO latestinfo (mac, seq, btn, lqi, batt, ticks) "
                + " values (" 
                + packet.mac + ", " 
                + packet.seq + ", "
                + packet.btn + ", "
                + packet.lqi + ", "
                + packet.batt + ", "
                + packet.dt.Ticks + ")";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                _updateDate();
            }
        }

        string lastUpdate = DateTime.Now.ToUniversalTime().ToString("r");
        private void _updateDate()
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


    }
}
