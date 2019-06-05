using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    /// <summary>
    /// 表示选项的详细描述
    /// </summary>
    public class DescriptionAttribute:SettingUIAttributeBase
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
