using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.Updater.PluginMarket
{
    public class PluginMarketPost
    {
        public string PluginName { get; set; }
        public string PluginAuthor { get; set; }
        public string Description { get; set; }

        public string ReleaseType { get; set; }
        public string ReleaseUrl { get; set; }

        public IEnumerable<PluginMarketRelease> ReleaseInfos { get; set; }
    }
}
