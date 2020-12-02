using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoBlazorCommon
{
    public class PanelModelGantt : PanelModel
    {
        public List<CycleTime> listCycle = new List<CycleTime>();

        public override void ClearPrevInfo()
        {
            base.ClearPrevInfo();
            listCycle.Clear();
        }

        public override void updateCycleTime(CycleTime ct)
        {
            base.updateCycleTime(ct);

            listCycle.Add(ct);
        }

        public static new PanelModel Create()
        {
            return new PanelModelGantt();
        }
    }
}
