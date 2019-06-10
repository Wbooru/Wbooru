using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class ListAttribute : SettingUIAttributeBase
    {
        public ListAttribute(bool @readonly,bool multi_select,bool ignore_case,string split_content,params string[] values)
        {
            ReadOnly = @readonly;
            MultiSelect = multi_select;
            CaseIgnore = ignore_case;
            SplitContent = split_content;
            Values = values;
        }

        public bool ReadOnly { get; }
        public bool MultiSelect { get; }
        public bool CaseIgnore { get; }
        public string SplitContent { get; }
        public string[] Values { get; }
    }
}
