using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YoloJsonClient
{
    public class YoloJson
    {
        public long frame_id { get; set; }

        public List<YoloJsonObject> objects { get; set; }

        public override string ToString()
        {
            var ans = $"fr:{frame_id}, obj_cnt:{objects.Count}, ";
            foreach(var obj in objects)
            {
                ans += $"name: {obj.name}, conf: {obj.confidence}"; 
            }

            return ans;
        }
    }
}
