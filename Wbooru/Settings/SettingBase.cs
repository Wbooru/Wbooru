using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings
{
    public abstract class SettingBase
    {
        public abstract void OnAfterLoad();
        public abstract void OnBeforeSave();
    }
}
