using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class PathAttribute:SettingUIAttributeBase
    {
        public PathAttribute(bool is_file_path,bool must_exist,string ext_filter=null,string default_ext = null)
        {
            IsFilePath = is_file_path;
            MustExist = must_exist;

            ExtFilter = ext_filter;
            DefaultExt = default_ext;
        }

        public bool IsFilePath { get; }
        public bool MustExist { get; }
        public string ExtFilter { get; }
        public string DefaultExt { get; }
    }
}
