using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings
{
    public class GlobalSetting : SettingBase
    {
        public string DownloadPath { get; set; } = "./Download";
        public int LoadingImageThread { get; set; } = 4;
    }
}
