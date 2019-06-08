using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings.UIAttributes;

namespace Wbooru.Models.SettingUI
{
    public class PropertyInfoWrapper
    {
        public PropertyInfo PropertyInfo { get; set; }
        public object OwnerObject { get; set; }

        public object ProxyValue
        {
            get => PropertyInfo.GetValue(OwnerObject);
            set => PropertyInfo.SetValue(OwnerObject, value);
        }

        public string DisplayPropertyName => PropertyInfo.GetCustomAttribute<NameAliasAttribute>()?.AliasName ?? PropertyInfo.Name;
    }
}
