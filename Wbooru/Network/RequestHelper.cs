using MihaZupan;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        private static IWebProxy socks5_proxy;

        private static HttpClient httpClient = new HttpClient(new HttpClientHandler { Proxy = TryGetAvaliableProxy() }) { Timeout = TimeSpan.FromSeconds(Setting<GlobalSetting>.Current.RequestTimeout) };

        private static GlobalSetting setting = Setting<GlobalSetting>.Current;

        private static IWebProxy TryGetAvaliableProxy()
        {
            if (setting == null || !setting.EnableSocks5Proxy)
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
                Log.Info($"Create sock5 proxy failed:" + e.Message);
                return null;
            }
        }

        public static HttpResponseMessage CreateDeafult(string url, Action<HttpRequestMessage> custom = null)
            => CreateDeafultAsync(url, custom).Result;

        public static Task<HttpResponseMessage> CreateDeafultAsync(string url, Action<HttpRequestMessage> custom) => CreateDeafultAsync(url, req =>
            {
                custom?.Invoke(req);
                return Task.CompletedTask;
            });

        public static async Task<HttpResponseMessage> CreateDeafultAsync(string url, Func<HttpRequestMessage, Task> custom = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Version = HttpVersion.Version20;

            if (custom?.Invoke(req) is Task task)
                await task;

            Log.Debug($"create http(s) {req.Method} request :{url}", "RequestHelper");

            return await httpClient.SendAsync(req);
        }

        public static string GetString(HttpResponseMessage response)
        {
            using var reader = new StreamReader(response.Content.ReadAsStream());

            return reader.ReadToEnd();
        }

        public static T GetJsonContainer<T>(HttpResponseMessage response) where T : JContainer
        {
            using var reader = new StreamReader(response.Content.ReadAsStream());

            try
            {
                return JsonConvert.DeserializeObject(reader.ReadToEnd()) as T;
            }
            catch (Exception e)
            {
                Log.Info($"Can't get json object from request : {response.RequestMessage.RequestUri.AbsoluteUri} , message : {e.Message}");
                return default;
            }
        }

        public static JObject GetJsonObject(HttpResponseMessage response) => GetJsonContainer<JObject>(response);
    }
}
