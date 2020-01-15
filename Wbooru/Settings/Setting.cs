using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings
{
    public static class Setting<T> where T : SettingBase
    {
        public static T Current => SettingManager.LoadSetting<T>();
        public static void Reset() => SettingManager.ResetSetting<T>();
        public static void ForceSave() => SettingManager.SaveSettingFile();
        public static void ForceReload() => SettingManager.LoadSettingFile();
    }
}
