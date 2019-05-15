using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Network
{
    public static class RequestHelper
    {
        public static WebResponse CreateDeafult(string url)
        {
            var req = WebRequest.Create(url);

            return req.GetResponse();
        }
    }
}
