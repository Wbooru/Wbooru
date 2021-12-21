using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;
using Wbooru.Settings;
using Wbooru.Utils;

namespace Wbooru.Kernel.ManagerImpl
{
    [PriorityExport(typeof(ICacheManager), Priority = 0)]
    internal class CacheManager : ICacheManager
    {
        string temporary_folder_path;

        public CacheManager()
        {
            try
            {
                temporary_folder_path = Setting<GlobalSetting>.Current.CacheFolderPath.Replace("%Temp%", Path.GetTempPath());
                Directory.CreateDirectory(temporary_folder_path);
            }
            catch (Exception e)
            {
                Log.Error("Failed to check&create tempoary cache folder:" + e.Message);
            }
        }

        private string GetCacheFilePath(string cacheKey) => Path.Combine(temporary_folder_path, cacheKey);

        public async Task PutCacheContent(string cacheKey, Stream stream)
        {
            if (stream is null || string.IsNullOrWhiteSpace(cacheKey) || !Setting<GlobalSetting>.Current.EnableFileCache)
                return;
            using var fileStream = File.OpenWrite(GetCacheFilePath(cacheKey));
            await stream?.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
        }

        public async Task<Stream> GetCacheContent(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey) || !Setting<GlobalSetting>.Current.EnableFileCache)
                return default;
            var path = GetCacheFilePath(cacheKey);
            if (File.Exists(path))
                return File.OpenRead(path);
            return default;
        }

        public async Task<(ulong used, ulong total)> GetCurrentCacheUsage()
        {
            if (!Setting<GlobalSetting>.Current.EnableFileCache)
                return (0, 0);

            var len = await Task.Run(() => new DirectoryInfo(temporary_folder_path).EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories).Select(x =>
            {
                try
                {
                    var file = new FileInfo(x.FullName);
                    return file.Length;
                }
                catch
                {
                    return 0;
                }
            }).Sum());

            return ((ulong)len, ulong.MaxValue);
        }

        public Task ClearAllCacheContent()
        {
            return Task.Run(() =>
            {
                foreach (var item in Directory.EnumerateFileSystemEntries(temporary_folder_path))
                {
                    if (File.Exists(item))
                    {
                        File.Delete(item);
                    }
                    else
                    {
                        Directory.Delete(item, true);
                    }
                }
            });
        }
    }
}
