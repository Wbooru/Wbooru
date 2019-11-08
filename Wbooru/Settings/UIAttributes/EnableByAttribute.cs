using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    /// <summary>
    /// 表示此选项是否能被更改，取决于另一个bool类型选项的值
    /// </summary>
    public class EnableByAttribute : SettingUIAttributeBase
    {
        public string SettingName { get; }
        public bool HideIfDisable { get; }

        public EnableByAttribute(string setting_name,bool hide_if_disable = false)
        {
            SettingName = setting_name;
            HideIfDisable = hide_if_disable;
        }
    }
}
