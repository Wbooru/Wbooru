using MihaZupan;
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
using Wbooru.Settings;

namespace Wbooru.Network
{
    public static class RequestHelper
    {
        static RequestHelper()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; 
        }

        public static WebResponse CreateDeafult(string url, Action<HttpWebRequest> custom = null) => CreateDeafultAsync(url, custom).Result;

        private static IWebProxy socks5_proxy;

        private static GlobalSetting setting = SettingManager.LoadSetting<GlobalSetting>();

        private static IWebProxy TryGetAvaliableProxy()
        {
            if (!setting.EnableSocks5Proxy)
                return null;

            if (socks5_proxy != null)
                return socks5_proxy;

            try
            {
                socks5_proxy = new HttpToSocks5Proxy(setting.Socks5ProxyAddress, setting.Socks5ProxyPort);

                if (socks5_proxy != null)
                    Log.Info($"Enable sock5 , addr:port -> {setting.Socks5ProxyAddress}:{setting.Socks5ProxyPort}");

                return socks5_proxy;
            }
            catch (Exception e)
            {
                Log.Info($"Create sock5 proxy failed:"+e.Message);
                return null;
            }
        }

        public static WebResponse CreateDeafult(string url, Action<HttpWebRequest> custom = null) 
            => CreateDeafultAsync(url, custom).ConfigureAwait(false).GetAwaiter().GetResult();

        public static Task<WebResponse> CreateDeafultAsync(string url, Action<HttpWebRequest> custom = null)
        {
            var req = WebRequest.Create(url);
            req.Proxy = TryGetAvaliableProxy();
            req.Method = "GET";

            custom?.Invoke(req as HttpWebRequest);

            Log.Debug($"[thread:{Thread.CurrentThread.ManagedThreadId}] create http(s) {req.Method} request :{url}", "RequestHelper");

            return req.GetResponseAsync();
        }

        public static string GetString(WebResponse response)
        {
            using var reader = new StreamReader(response.GetResponseStream());

            return reader.ReadToEnd();
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
