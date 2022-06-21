using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wbooru.Network;
using Wbooru.UI.Controls;
using Wbooru.Utils;

namespace Wbooru.Kernel.Updater.PluginMarket
{
    [Export(typeof(PluginMarket))]
    public class DefaultPluginMarket : PluginMarket
    {
        public override string MarketName => "Offical Default Plugin Market";

        public override IEnumerable<PluginMarketPost> GetPluginPosts()
        {
            JArray plugin_issues;

            try
            {
                plugin_issues = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult("https://api.github.com/repos/Wbooru/Wbooru.PluginsMarket/issues?labels=Plugin-Release",req=> req.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Wbooru", "1.1.0"))));
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                yield break;
            }

            if (plugin_issues != null)
            {
                foreach (var post in plugin_issues)
                {
                    yield return BuildPluginMarketPost(post);
                }
            }
        }

        private PluginMarketPost BuildPluginMarketPost(JToken post_json)
        {
            var post = UpdaterHelper.BuildInstance<PluginMarketPost>(post_json);

            if ("GithubRelease".Equals(post.ReleaseType,StringComparison.InvariantCultureIgnoreCase))
            {
                post.ReleaseInfos = UpdaterHelper.GetPostReleaseInfosFromGithubRelease(post.ReleaseUrl).ToArray();
            }
            else
            {
                post.ReleaseInfos = UpdaterHelper.GetPostReleaseInfosFromIssueCommentsAPI(post_json["comments_url"].ToString(), post_json["user"]["id"].ToString()).ToArray();
                post.ReleaseUrl = string.IsNullOrWhiteSpace(post.ReleaseUrl) ? post_json["html_url"].ToString() : post.ReleaseUrl;
            }

            //adjus post.ReleaseInfos
            var currentRequiredWbooruMinVersion = new Version(0, 0, 0, 0);
            var currentWbooruVersion = ProgramUpdater.CurrentProgramVersion;

            post.ReleaseInfos = post.ReleaseInfos.OrderBy(x=>x.Version).Select(x =>
            {
                var wmv = x.RequestWbooruMinVersion ?? currentRequiredWbooruMinVersion;
                if (wmv > currentRequiredWbooruMinVersion)
                    currentRequiredWbooruMinVersion = wmv;
                x.RequestWbooruMinVersion = wmv;
                return x;
            }).ToArray();

            return post;
        }
    }

    public class VersionSimpleTypeConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value?.ToString() ?? String.Empty;

            if (Version.TryParse(str, out var version))
                return version;

            return base.ConvertFrom(context, culture, value);
        }
    }
}
