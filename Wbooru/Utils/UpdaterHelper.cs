using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wbooru.Kernel.Updater;
using Wbooru.Kernel.Updater.PluginMarket;
using Wbooru.Network;

namespace Wbooru.Utils
{
    public static class UpdaterHelper
    {
        static UpdaterHelper()
        {
            TypeDescriptor.AddAttributes(typeof(Version), new TypeConverterAttribute(typeof(VersionSimpleTypeConverter)));
        }

        public static IEnumerable<ReleaseInfo> GetGithubAllReleaseInfoList(string owner, string repo) => GetGithubAllReleaseInfoList(BuildGithubReleaseApiUrl(owner, repo));

        public static IEnumerable<ReleaseInfo> GetGithubAllReleaseInfoList(string github_releases_url)
        {
            var array = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(github_releases_url, req =>
            {
                req.UserAgent = "Wbooru";
            }));

            return array
                .Select(x => TryGetReleaseInfo(x))
                .OfType<ReleaseInfo>().ToArray();
        }

        private static ReleaseInfo TryGetReleaseInfo(JToken x)
        {
            try
            {
                return new ReleaseInfo
                {
                    ReleaseDescription = x["body"].ToString(),
                    ReleaseDate = DateTime.Parse(x["published_at"].ToString()),
                    ReleaseType = x["prerelease"].ToObject<bool>() ? ReleaseType.Preview : ReleaseType.Stable,
                    ReleaseURL = x["html_url"].ToString(),
                    Version = Version.Parse(x["tag_name"].ToString().TrimStart('v')),
                    DownloadURL = ((x["assets"] as JArray)?.FirstOrDefault()["browser_download_url"])?.ToString()
                };
            }
            catch (Exception e)
            {
                Log.Error($"Can't parse release info from json content: {e.Message} \n {x.ToString()}");
                return null;
            }
        }

        public static (string owner,string repo_name) ParseOwnerAndRepoFromUrl(string github_link)
        {
            var match = Regex.Match(github_link, @"^.*github\.com/([^/]+)/([^/]+).*$");

            if (!match.Success)
                return default;

            var owner = match.Groups[1].Value;
            var repo = match.Groups[2].Value;

            Log.Debug($"github_link = {github_link} | owner = {owner} | repo = {repo}");

            return (owner,repo);
        }

        public static string BuildGithubReleaseApiUrl(string owner, string repo) => $"https://api.github.com/repos/{owner}/{repo}/releases";

        public static IEnumerable<PluginMarketRelease> GetPostReleaseInfosFromGithubRelease(string githubRepoUrl)
        {
            var (owner, repo) = ParseOwnerAndRepoFromUrl(githubRepoUrl);

            var github_release_api = BuildGithubReleaseApiUrl(owner, repo);

            Log.Debug($"{githubRepoUrl} -> {github_release_api}");

            var raw_release_infos = GetGithubAllReleaseInfoList(github_release_api).Select(x => new PluginMarketRelease()
            {
                DownloadURL = x.DownloadURL,
                ReleaseDate = x.ReleaseDate,
                ReleaseDescription = x.ReleaseDescription,
                ReleaseType = x.ReleaseType,
                Version = x.Version,
                ReleaseURL = x.ReleaseURL
            });

            return raw_release_infos;
        }

        public static T BuildInstance<T>(JToken post_json) where T : new()
        {
            var body = post_json["body"].ToString();

            var props = Regex.Matches(body, @"^(\w*)\|(.*)$", RegexOptions.Multiline)
                .OfType<Match>()
                .Select(x => (x.Groups[1].Value.Trim(), x.Groups[2].Value.Trim()))
                .Where(x => !x.Item1.Equals("Property", StringComparison.InvariantCultureIgnoreCase));

            var prop_info = typeof(T).GetProperties();

            var post = new T();

            foreach (var prop in props)
            {
                if (prop_info.FirstOrDefault(x => x.Name.Equals(prop.Item1, StringComparison.InvariantCultureIgnoreCase)) is PropertyInfo pi &&
                    TypeDescriptor.GetConverter(pi.PropertyType) is TypeConverter converter)
                {
                    var value = converter.ConvertFromString(prop.Item2);
                    pi.SetValue(post, value);
                }
            }

            return post;
        }

        public static IEnumerable<PluginMarketRelease> GetPostReleaseInfosFromIssue(string url)
        {
            try
            {
                var api_url = url.Contains("api.github.com") ? url : url.Replace("github.com", "api.github.com/repos");
                Log.Debug($"{url} -> {api_url}");

                var issue_json = RequestHelper.GetJsonContainer<JObject>(RequestHelper.CreateDeafult(api_url, req => req.UserAgent = "Wbooru"));
                return GetPostReleaseInfosFromIssueCommentsAPI(issue_json["comments_url"].ToString(), issue_json["user"]["id"].ToString());
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                return Enumerable.Empty<PluginMarketRelease>();
            }
        }

        public static IEnumerable<PluginMarketRelease> GetPostReleaseInfosFromIssueCommentsAPI(string url,string issue_author)
        {
            JArray release_infos;

            try
            {
                release_infos = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(url, req => req.UserAgent = "Wbooru"));
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                yield break;
            }

            if (release_infos != null)
            {
                Log.Info($"url = {url} , issue_author id = {issue_author}");
                foreach (var post in release_infos.Where(x => x["user"]["id"].ToString() == issue_author))
                {
                    Log.Debug($"pick : {release_infos.ToArray()}");

                    var info = BuildInstance<PluginMarketRelease>(post);

                    info.ReleaseDate = post["created_at"].ToObject<DateTime>();
                    info.ReleaseURL = post["html_url"].ToString();

                    if (CheckReleaseInfoVailed(info))
                        yield return info;
                }
            }

            bool CheckReleaseInfoVailed(PluginMarketRelease info)
            {
                return (!string.IsNullOrWhiteSpace(info.DownloadURL)) &&
                    info.Version != null;
            }
        }
    }
}
