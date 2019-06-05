using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public static class UIAttributeExtension
    {
        public static IEnumerable<SettingUIAttributeBase> GetUIAttributes(this SettingBase setting)
        {
            return setting.GetType().GetCustomAttributes(typeof(SettingUIAttributeBase),true).OfType<SettingUIAttributeBase>();
        }
    }
}
