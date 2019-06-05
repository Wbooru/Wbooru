using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class NameAliasAttribute:SettingUIAttributeBase
    {
        public NameAliasAttribute(string alias_name)
        {
            AliasName = alias_name;
        }

        public string AliasName { get; }
    }
}
