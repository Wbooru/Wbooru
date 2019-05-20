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
        public static WebResponse CreateDeafult(string url)
        {
            var req = WebRequest.Create(url);

            Log.Debug($"[thread:{Thread.CurrentThread.ManagedThreadId}]create http(s) request :{url}", "RequestHelper");

            return req.GetResponse();
        }
    }
}
