using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;

namespace Wbooru.Settings
{
    public class SettingBase : IMultiImplementInjectable
    {
        public virtual void OnAfterLoad() { }
        public virtual void OnBeforeSave() { }
    }
}
