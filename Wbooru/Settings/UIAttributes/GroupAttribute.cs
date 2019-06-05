using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    /// <summary>
    /// 表示一个选项所属的选项组
    /// </summary>
    public class GroupAttribute:SettingUIAttributeBase
    {
        public GroupAttribute(string group_name)
        {
            GroupName = group_name;
        }

        public string GroupName { get; }
    }
}
