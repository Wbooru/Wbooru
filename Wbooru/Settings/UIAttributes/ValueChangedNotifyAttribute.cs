using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings.UIAttributes
{
    public class ValueChangedNotifyAttribute : SettingUIAttributeBase
    {
        public ValueChangedNotifyAttribute(string ReflectCallbackMethodName)
        {
            this.ReflectCallbackMethodName = ReflectCallbackMethodName;
        }

        public string ReflectCallbackMethodName { get; }

        public MethodInfo MethodInfo { get; set; }
    }
}
