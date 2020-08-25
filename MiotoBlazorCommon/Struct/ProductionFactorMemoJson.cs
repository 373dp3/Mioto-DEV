using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoBlazorCommon.Struct
{
    public class ProductionFactorMemoJson
    {
        public string itemNumber { get; set; } = "";
        public string operatorName { get; set; } = "";

        public void Copy(ProductionFactorMemoJson obj)
        {
            this.itemNumber = obj.itemNumber;
            this.operatorName = obj.operatorName;
        }
    }

}
