using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.Updater;

namespace Wbooru.PluginExt
{
    public interface IPluginUpdatable
    {
        Version CurrentPluginVersion { get; }

        IEnumerable<ReleaseInfo> GetReleaseInfoList();
    }
}
