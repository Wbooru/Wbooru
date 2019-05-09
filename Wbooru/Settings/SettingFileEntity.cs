using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Settings
{
    internal class SettingFileEntity
    {
        public Dictionary<string,SettingBase> Settings { get; set; } = new Dictionary<string,SettingBase>();
    }
}
