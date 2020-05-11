using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloJsonClient
{
    public class YoloJsonObject
    {
        public int class_id { get; set; }
        public string name { get; set; }

        public YoloJsonObjectRect relative_coordinates { get; set; }

        public float confidence { get; set; }
    }

    public class YoloJsonObjectRect
    {
        public float center_x { get; set; }
        public float center_y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    }
}
