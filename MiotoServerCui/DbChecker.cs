/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    class DbChecker
    {
        /*
         * 起動時に稼働上の日付変更時刻を受け取り、時刻をまたいだ場合にDBの
         * メンテコマンドを起動する
         * 
         * [TODO]運用規模が大きくなった段階で、日付切替時に旧日付をtxtに出力し、
         * DBの過去データ消去、バキューム処理を実施したいところ。
         * 
         * */
        public int hhmm { get; private set; }
        private string strDtHHMMSS = "";
        private int preHHMM;
        private static DbChecker instance = null;
        public static DbChecker getInstance(string hhmmStr=null)
        {
            if(instance != null) { return instance; }
            if(hhmmStr ==null) { throw new ArgumentException("初回呼び出し時には時刻情報が必要です"); }
            instance = new DbChecker(hhmmStr);
            return instance;
        }
        private DbChecker(string hhmmStr)
        {
            preHHMM = getCurrentHHMM_Int();
            try
            {
                hhmm = Convert.ToInt32(hhmmStr);
                if (hhmm > 2359) { hhmm = 0; }
                var hh = (int)(hhmm / 100);
                var mm = hhmm - 100 * hh;
                strDtHHMMSS = String.Format("{0:D2}:{1:D2}:00", hh, mm);
            }
            finally { }

            //DBのDate2Rowを確認。PCの起動の段階で日付をまたいでいるかをチェック
            var p = DbWrapper.getInstance();
            {
                var tick = p.getMaxTick();
                if (tick == 0) { return; }
                //DB内のCSVキャッシュ最終日時から、そのデータの職務日付変更日時を算出
                //現在時刻が、該当データの職務日付変更日時を超えていた場合、Date2Rowを更新
                var finalDt = new DateTime(tick);
                var finalDtHHMM = Convert.ToInt32(finalDt.ToString("HHmm"));
                var hh = (int)(hhmm / 100);
                var mm = hhmm - 100 * hh;
                var workEndDt = DateTime.Parse(finalDt.ToString("yyyy/MM/dd") + String.Format(" {0:D2}:{1:D2}:00", hh, mm));
                workEndDt = workEndDt.AddDays(1);//1加算することで職務終了日時に修正
                //0:00を超えているが職務日付変更時刻を超えていない場合は、日付を一つ戻す
                if ((finalDtHHMM < hhmm))
                {
                    workEndDt = workEndDt.AddDays(-1);
                }
                //PCの現在時刻が最終データに対する職務日付変更時刻を超えている場合は、
                //Date2Rowを更新
                if (DateTime.Now > workEndDt)
                {
                    //日付をまたいでいるのでテーブルに情報追加
                    p.updateDate2Row(workEndDt.ToString("yyyyMMdd"));
                }
            }
        }

        public DateTime getDateTimeOfWorkDay(List<UInt32> ymdList, bool isStart=true)
        {
            //指定時刻と現在時刻を比較。職務上の切り替え時刻より
            //値が若い場合は夜勤として職務上の開始日時を1日前にする。
            var dt = DateTime.Now;
            if((ymdList!=null) && (ymdList.Count > 0))
            {
                int pos = 0;
                if((isStart==false) && (ymdList.Count > 1))
                {
                    pos++;
                }
                var ymd = ymdList[pos];
                long yyyy = (long)(ymd/10000);
                long mm = (long)((ymd-yyyy*10000)/100);
                long dd = ymd - yyyy * 10000 - mm * 100;
                dt = DateTime.Parse(String.Format("{0:D4}/{1:D2}/{2:D2} 0:00:00", yyyy,mm,dd));
            }
            if (isStart == false)
            {
                dt = dt.AddDays(1);
            }
            if (getCurrentHHMM_Int() < this.hhmm)
            {
                dt = dt.AddDays(-1);
            }
            dt = DateTime.Parse(dt.ToString("yyyy/MM/dd") + " " + this.strDtHHMMSS);
            return dt;
        }
        public void fetch()
        {
            var curHHMM = getCurrentHHMM_Int();
            if((preHHMM < hhmm) && (hhmm <= curHHMM))
            {
                DbWrapper.getInstance().updateDate2Row(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
            }
            //次回起動用
            preHHMM = curHHMM;
        }

        private int getCurrentHHMM_Int()
        {
            return Convert.ToInt32(DateTime.Now.ToString("HHmm"));
        }
    }
}
