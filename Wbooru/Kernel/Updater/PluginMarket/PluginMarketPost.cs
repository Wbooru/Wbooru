using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;

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

        public IEnumerable<PluginMarketRelease> SuitableReleaseInfos => ReleaseInfos
            .Where(x => x.ReleaseType == Updater.ReleaseType.Stable || (x.ReleaseType == Updater.ReleaseType.Preview && Setting<GlobalSetting>.Current.UpdatableTargetVersion == GlobalSetting.UpdatableTarget.Preview))
            .Where(x => ProgramUpdater.CurrentProgramVersion >= x.RequestWbooruMinVersion);

        public PluginMarketRelease CurrentSuitableRelease => SuitableReleaseInfos.FirstOrDefault();
        public PluginMarketRelease LatestRelease => ReleaseInfos.OrderByDescending(x => x.ReleaseDate).FirstOrDefault();

        public override string ToString() => $"Name={PluginName}, Author={PluginAuthor}, ReleaseType={ReleaseType}";
    }
}
