using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.ProgramUpdater;
using Wbooru.Network;

namespace Wbooru.Utils
{
    public static class UpdaterHelper
    {
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
                    Description = x["body"].ToString(),
                    ReleaseDate = DateTime.Parse(x["published_at"].ToString()),
                    Type = x["prerelease"].ToObject<bool>() ? ReleaseInfo.ReleaseType.Preview : ReleaseInfo.ReleaseType.Stable,
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
    }
}
