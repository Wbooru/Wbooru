using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.Network
{
    public static class RequestHelper
    {
        public static WebResponse CreateDeafult(string url, Action<HttpWebRequest> custom = null) => CreateDeafultAsync(url, custom).Result;

        public static Task<WebResponse> CreateDeafultAsync(string url, Action<HttpWebRequest> custom = null)
        {
            var req = HttpWebRequest.Create(url);

            custom?.Invoke(req as HttpWebRequest);

            Log.Debug($"[thread:{Thread.CurrentThread.ManagedThreadId}]create http(s) async request :{url}", "RequestHelper");

            return req.GetResponseAsync();
        }

        public static T GetJsonContainer<T>(WebResponse response) where T : JContainer
        {
            using var reader = new StreamReader(response.GetResponseStream());

            try
            {
                return JsonConvert.DeserializeObject(reader.ReadToEnd()) as T;
            }
            catch (Exception e)
            {
                Log.Info($"Can't get json object from request : {response.ResponseUri.AbsoluteUri} , message : {e.Message}");
                return default;
            }
        }

        public static JObject GetJsonObject(WebResponse response) => GetJsonContainer<JObject>(response);
    }
}
