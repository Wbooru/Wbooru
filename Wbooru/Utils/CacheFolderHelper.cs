using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;

namespace Wbooru.Utils
{
    public static class CacheFolderHelper
    {
        private static string cache_folder = null;

        public static string CacheFolderPath
        {
            get
            {
                if (cache_folder == null)
                {
                    cache_folder = Setting<GlobalSetting>.Current.CacheFolderPath.Replace("%Temp%", Path.GetTempPath());
                    Directory.CreateDirectory(cache_folder);
                }

                return cache_folder;
            }
        }
    }
}
