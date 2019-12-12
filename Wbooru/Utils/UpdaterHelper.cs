using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wbooru.Kernel.Updater;
using Wbooru.Network;

namespace Wbooru.Utils
{
    public static class UpdaterHelper
    {
        public static IEnumerable<ReleaseInfo> GetGithubAllReleaseInfoList(string owner, string repo) => GetGithubAllReleaseInfoList(BuildGithubReleaseApiUrl(owner, repo));

        public static IEnumerable<ReleaseInfo> GetGithubAllReleaseInfoList(string github_releases_url)
        {
            var array = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(github_releases_url, req =>
            {
                req.UserAgent = "WbooruProgramUpdater";
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
    }
}
