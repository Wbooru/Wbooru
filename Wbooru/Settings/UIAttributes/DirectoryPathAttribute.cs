using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class DirectoryPathAttribute:SettingUIAttributeBase
    {
        public DirectoryPathAttribute(string dir_path,bool must_exist)
        {
            DirectoryPath = dir_path;
            MustExist = must_exist;
        }

        public string DirectoryPath { get; }
        public bool MustExist { get; }
    }
}
