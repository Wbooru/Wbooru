using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Wbooru.Settings.UIAttributes
{
    public class ListAttribute : SettingUIAttributeBase
    {
        public ListAttribute(Type enum_type, bool ignore_case, string split_content)
        {
            MultiSelect = enum_type.GetCustomAttribute<FlagsAttribute>() != null;
            Values = Enum.GetNames(enum_type);
            CaseIgnore = ignore_case;
            SplitContent = split_content;
        }

        public ListAttribute(bool multi_select,bool ignore_case,string split_content,params string[] values)
        {
            MultiSelect = multi_select;
            CaseIgnore = ignore_case;
            SplitContent = split_content;
            Values = values;
        }

        public bool MultiSelect { get; }
        public bool CaseIgnore { get; }
        public string SplitContent { get; }
        public string[] Values { get; }
    }
}
