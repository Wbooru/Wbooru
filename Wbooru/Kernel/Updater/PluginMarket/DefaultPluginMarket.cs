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
using Wbooru.Utils;

namespace Wbooru.Kernel.Updater.PluginMarket
{
    [Export(typeof(PluginMarket))]
    public class DefaultPluginMarket : PluginMarket
    {
        static DefaultPluginMarket()
        {
            TypeDescriptor.AddAttributes(typeof(Version), new TypeConverterAttribute(typeof(VersionSimpleTypeConverter)));
        }

        public override string MarketName => "Offical Default Plugin Market";

        public override IEnumerable<PluginMarketPost> GetPluginPosts()
        {
            JArray plugin_issues;

            try
            {
                plugin_issues = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult("https://api.github.com/repos/MikiraSora/Wbooru.PluginsMarket/issues?labels=Plugin-Release",req=>req.UserAgent="WbooruPluginMarket"));
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
            var post = BuildInstance<PluginMarketPost>(post_json);

            if ("GithubRelease".Equals(post.ReleaseType,StringComparison.InvariantCultureIgnoreCase))
            {
                post.ReleaseInfos = GetPostReleaseInfosFromGithubRelease(post.ReleaseUrl).ToArray();
            }
            else
            {
                post.ReleaseInfos = GetPostReleaseInfos(post_json["comments_url"].ToString()).ToArray();
                post.ReleaseUrl = string.IsNullOrWhiteSpace(post.ReleaseUrl) ? post_json["html_url"].ToString() : post.ReleaseUrl;
            }

            return post;
        }

        private IEnumerable<PluginMarketRelease> GetPostReleaseInfosFromGithubRelease(string githubRepoUrl)
        {
            var (owner, repo) = UpdaterHelper.ParseOwnerAndRepoFromUrl(githubRepoUrl);

            var github_release_api = UpdaterHelper.BuildGithubReleaseApiUrl(owner, repo);

            Log.Debug($"{githubRepoUrl} -> {github_release_api}");

            var raw_release_infos = UpdaterHelper.GetGithubAllReleaseInfoList(github_release_api).Select(x=>new PluginMarketRelease() {
                DownloadURL = x.DownloadURL,
                ReleaseDate = x.ReleaseDate,
                ReleaseDescription = x.ReleaseDescription,
                ReleaseType = x.ReleaseType,
                Version = x.Version,
                ReleaseURL = x.ReleaseURL
            });

            return raw_release_infos;
        }

        private T BuildInstance<T>(JToken post_json) where T:new()
        {
            var body = post_json["body"].ToString();

            var props = Regex.Matches(body, @"^(\w*)\|(.*)$",RegexOptions.Multiline)
                .OfType<Match>()
                .Select(x=>(x.Groups[1].Value.Trim(), x.Groups[2].Value.Trim()))
                .Where(x=>!x.Item1.Equals("Property", StringComparison.InvariantCultureIgnoreCase));

            var prop_info = typeof(T).GetProperties();

            var post = new T();

            foreach (var prop in props)
            {
                if (prop_info.FirstOrDefault(x=>x.Name.Equals(prop.Item1,StringComparison.InvariantCultureIgnoreCase)) is PropertyInfo pi &&
                    TypeDescriptor.GetConverter(pi.PropertyType) is TypeConverter converter)
                {
                    var value = converter.ConvertFromString(prop.Item2);
                    pi.SetValue(post, value);
                }
            }

            return post;
        }

        private IEnumerable<PluginMarketRelease> GetPostReleaseInfos(string url)
        {
            JArray release_infos;

            try
            {
                release_infos = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(url, req => req.UserAgent = "WbooruPluginMarket"));
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                yield break;
            }

            if (release_infos != null)
            {
                foreach (var post in release_infos.Where(x => x["author_association"].ToString() == "OWNER"))
                {
                    var info = BuildInstance<PluginMarketRelease>(post);

                    info.ReleaseDate = post["created_at"].ToObject<DateTime>();
                    info.ReleaseURL = post["html_url"].ToString();

                    if (CheckReleaseInfoVailed(info))
                    {
                        yield return info;
                    }
                }
            }
        }

        private bool CheckReleaseInfoVailed(PluginMarketRelease info)
        {
            return (!string.IsNullOrWhiteSpace(info.DownloadURL)) &&
                info.Version!=null;
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
