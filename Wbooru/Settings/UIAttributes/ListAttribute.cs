using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class ListAttribute : SettingUIAttributeBase
    {
        public ListAttribute(bool multi_select,bool ignore_case,string split_content=",")
        {
            MultiSelect = multi_select;
            CaseIgnore = ignore_case;
            SplitContent = split_content;
        }

        public bool MultiSelect { get; }
        public bool CaseIgnore { get; }
        public string SplitContent { get; }
    }
}
