using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class RangeAttribute:SettingUIAttributeBase
    {
        public RangeAttribute(string min_value, string max_value)
        {
            Min = min_value;
            Max = max_value;
        }

        public string Min { get; }
        public string Max { get; }
    }
}
