using System;
using System.Collections.Generic;
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
    }
}
