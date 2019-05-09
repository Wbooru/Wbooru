using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;

namespace Wbooru.Utils.Resource
{
    public static class ResourceManager
    {
        private static ObjectCache cache = MemoryCache.Default;
        private static readonly string TEMP_PART = Path.GetTempPath();

        public static object RequestResource(string resource_name,Func<object> manual_request)
        {
            if (TryGetResourceFromMemoryCache(resource_name,out var res))
                return res;

            if (TryGetResourceFromDownloadFolder(resource_name, out res))
                return res;

            if (manual_request() is object obj)
            {
                cache[resource_name] = obj;
                return obj;
            }

            return null;
        }

        private static bool TryGetResourceFromDownloadFolder(string name, out object res)
        {
            res = null;

            //todo
            return false;
        }

        private static bool TryGetResourceFromMemoryCache(string name, out object res)
        {
            res = null;

            if (cache.Contains(name))
                return (res=cache[name])!=null;

            return false;
        }
    }
}
