using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoBlazorCommon.Struct
{
    public class ProductionFactorMemoJson
    {
        public string itemNumber { get; set; } = "";//品番
        public string operatorName { get; set; } = "";

        [Required]
        [Range(1,1000, ErrorMessage ="1～1000の間で入力してください")]
        public int itemsPerOperation { get; set; } = 1;//取数(1回あたりの出来高)

        public void Copy(ProductionFactorMemoJson obj)
        {
            this.itemNumber = obj.itemNumber;
            this.operatorName = obj.operatorName;
            this.itemsPerOperation = obj.itemsPerOperation;
        }
    }

}
