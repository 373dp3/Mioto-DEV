using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class ButtonAttr
    {
        public long mac { get; set; } = 0;
        public double ct { get; set; } = ProductionFactor.CT_NOOP;
        public string name { get; private set; } = "-";
        public string text { get; private set; } = null;
        public string classStr { get; private set; } = "";
        public ProductionFactor.Status status { get; set; } = ProductionFactor.Status.NOOP;
        private ButtonAttr() { }
        public ButtonAttr(ProductionFactor.Status status, Action<ProductionFactor> onclick = null)
        {
            this.status = status;
            load();
            this.OnClick = onclick;
        }
        public Action<ProductionFactor> OnClick { get; set; } = null;
        private void load()
        {
            //Modal, Buttonともに使用することを想定。bg～、btn～
            classStr = $"{GetBgCode(status)} {GetTextCode(status)}";

            name = ProductionFactor.GetStatusStr(status);
            if(text==null) text = $"「{name}」を登録してもよろしいでしょうか？";
        }
        public static string GetBgCode(ProductionFactor.Status status)
        {
            switch (status)
            {
                case ProductionFactor.Status.NOOP:
                    return "-muted ";
                case ProductionFactor.Status.START_PRODUCTION:
                    return "-primary ";
                case ProductionFactor.Status.WAITING_FOR_PARTS:
                    return "-warning ";
                case ProductionFactor.Status.START_REST:
                    return "-success ";
                case ProductionFactor.Status.START_PLANNED_STOP:
                    return "-dark ";

                //ライン外
                case ProductionFactor.Status.START_CHANGE_PRODUCTION:
                    return "-secondary ";
                case ProductionFactor.Status.FINISH_CHANGE_PRODUCTION:
                    return "-dark ";


                //非常時
                case ProductionFactor.Status.START_BRAKEDOWN_STOP:
                    return "-danger ";
                case ProductionFactor.Status.START_REPAIRING:
                    return "-info ";
                case ProductionFactor.Status.FINISH_REPAIRING:
                    return "-dark ";
            }
            return "";
        }
        public static string GetTextCode(ProductionFactor.Status status)
        {
            switch (status)
            {
                case ProductionFactor.Status.NOOP:
                    return " ";
                case ProductionFactor.Status.WAITING_FOR_PARTS:
                    return " ";
            }
            return " text-white ";
        }
    }
}
