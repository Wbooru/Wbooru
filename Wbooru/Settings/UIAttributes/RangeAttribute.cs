using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class RangeAttribute:SettingUIAttributeBase
    {
        public RangeAttribute(double min_value, double max_value)
        {
            Min = min_value.ToString();
            Max = max_value.ToString();
        }

        public RangeAttribute(int min_value, int max_value)
        {
            Min = min_value.ToString();
            Max = max_value.ToString();
        }

        public string Min { get; }
        public string Max { get; }
    }
}
