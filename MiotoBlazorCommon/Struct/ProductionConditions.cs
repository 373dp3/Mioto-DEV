using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MiotoBlazorCommon.DB;
using System.Text.Json.Serialization;
using System;

namespace MiotoBlazorCommon.Struct
{
    public class ProductionConditions
    {
        [Required]
        [StringLength(500, ErrorMessage = "文字数が超過しています(500バイトまで)")]
        public string itemNumber { get; set; } = "";

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "1以上の値(半角数字)が必要です")]
        public double standardCt { get; set; } = 20.0;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "1以上の値(半角数字)が必要です")]
        public int itemsPerOperation { get; set; } = 1;

        [JsonIgnore]
        public int metrix { get; set; } = 0;

        public void Copy(ProductionConditions obj)
        {
            this.itemNumber = obj.itemNumber;
            this.standardCt = obj.standardCt;
            this.itemsPerOperation = obj.itemsPerOperation;
        }

        public string ToTSV()
        {
            return itemNumber + '\t' + standardCt + '\t' + itemsPerOperation;
        }
        public static bool operator ==(ProductionConditions a, ProductionConditions b)
        {
            if(a.ToTSV().CompareTo(b.ToTSV())==0) { return true; }
            return false;
        }
        public static bool operator !=(ProductionConditions a, ProductionConditions b)
        {
            if (a.ToTSV().CompareTo(b.ToTSV()) == 0) { return false; }
            return true;
        }

        public ProductionConditions() { }
        public ProductionConditions(ProductionConditions obj)
        {
            this.Copy(obj);
        }
        public ProductionConditions(string tsv)
        {
            //Excelのセルのコピペを想定。タブ区切り
            //itemNumber    ct  itemPerOpe
            var work = tsv.Replace("\r","").Replace("\n","");
            var ary = work.Split('\t');
            if (ary.Length < 2) { throw new FormatException("要素が不足しています"); }
            try
            {
                this.itemNumber = ary[0];
                this.standardCt = Convert.ToDouble(ary[1]);
            }
            catch (Exception e)
            {
                throw new FormatException("文字列を数値に変換できませんでした");
            }

            if ((ary.Length >= 3) && (ary[2].Length>0))
            {
                try
                {
                    this.itemsPerOperation = Convert.ToInt32(ary[2]);
                }
                catch (Exception e)
                {
                    throw new FormatException("文字列を数値に変換できませんでした");
                }
            }

        }
    }
}
