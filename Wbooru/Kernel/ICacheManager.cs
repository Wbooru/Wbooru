using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;

namespace Wbooru.Kernel
{
    public interface ICacheManager : IImplementInjectable
    {
        public Task<Stream> GetCacheContent(string cacheKey);
        public Task<(ulong used,ulong total)> GetCurrentCacheUsage();
        public Task PutCacheContent(string cacheKey, Stream stream);
        public Task ClearAllCacheContent();
    }
}
