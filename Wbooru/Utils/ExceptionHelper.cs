using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wbooru.UI.Controls;

namespace Wbooru.Utils
{
    public static class ExceptionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugThrow(Exception e)
        {
#if DEBUG
            if (e.InnerException is WebException ne && (ne.Status == (WebExceptionStatus)403|| ne.Status == WebExceptionStatus.ProtocolError) && ne.Response.ResponseUri.AbsoluteUri.Contains("api.github.com"))
            {
                //fuck github api limit 
                Log.Error($"Can't access github api ({ne.Response.ResponseUri.AbsoluteUri}) because of api request limit.");
                return;
            }

            throw new AggregateException($"DebugThrow :{e.Message}", e);
#endif

#pragma warning disable CS0162
            Log.Error("Caught a exception but program needn't exit:" + e.Message + Environment.NewLine + e.StackTrace);
#pragma warning restore CS0162
        }
    }
}
