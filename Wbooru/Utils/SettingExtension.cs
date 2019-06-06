using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings.UIAttributes;

namespace Wbooru.Settings
{
    public static class SettingExtension
    {
        public static string GetSettingPropDisplayName(this PropertyInfo prop_info) => prop_info.GetCustomAttribute<NameAliasAttribute>()?.AliasName ?? prop_info.Name;
    }
}
