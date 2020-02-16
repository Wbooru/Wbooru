using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wbooru.PluginExt;
using Wbooru.Settings;

namespace Wbooru.Utils.Resource
{
    public static class ImageResourceManager
    {
        private static GlobalSetting option;
        private static string temporary_folder_path;
        private static long current_record_capacity;

        public static void InitImageResourceManager()
        {
            option = SettingManager.LoadSetting<GlobalSetting>();

            if (option.EnableFileCache)
            {
                try
                {
                    temporary_folder_path = CacheFolderHelper.CacheFolderPath;

                    current_record_capacity = Directory.EnumerateFiles(temporary_folder_path, "*.cache").Select(x =>
                    {
                        using var stream = File.OpenRead(x);
                        return stream.Length;
                    }).Sum();

                    Log.Info($"Check&Create tempoary cache folder({current_record_capacity}):{temporary_folder_path}");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to check&create tempoary cache folder:" + e.Message);
                }
            }
        }

        public static async Task<Image> RequestImageAsync(string resource_name,Func<Task<Image>> manual_request)
        {
            var hash = resource_name.GetHashCode().ToString();
            Log.Debug($"Convert Hash:{resource_name} -> {hash}");
            resource_name = hash;

            if (TryGetImageFromTempFolder(resource_name, out var res))
            {
                Log.Debug("Get cache image resoure from temporary folder : " + resource_name);
                return res;
            }

            if (TryGetImageFromDownloadFolder(resource_name, out res))
            {
                Log.Debug("Get image resoure from download folder : " + resource_name);
                return res;
            }

            if (await manual_request() is Image obj)
            {
                CacheImageResourceAsFile(resource_name, obj);

                return obj;
            }

            return null;
        }

        private static bool TryGetImageFromDownloadFolder(string name, out Image res)
        {
            res = null;

            //name = FileNameHelper.FilterFileName(name);

            var file_path = Path.Combine(SettingManager.LoadSetting<GlobalSetting>().DownloadPath, name);

            if (!File.Exists(file_path))
                return false;

            res = Image.FromFile(file_path);

            //todo
            return false;
        }

        #region Temp Folder Cache

        private static bool TryGetImageFromTempFolder(string resource_name, out Image res)
        {
            res = null;

            if (!option.EnableFileCache || temporary_folder_path == null)
                return false;

            //resource_name = FileNameHelper.FilterFileName(resource_name);
            resource_name = resource_name.EndsWith(".cache") ? resource_name : (resource_name + ".cache");
            var file_path = Path.Combine(temporary_folder_path, resource_name);

            if (File.Exists(file_path))
            {
                try
                {
                    res = Image.FromFile(file_path);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Debug("Failed to load cache image from cache folder:" + e.Message);
                    return false;
                }
            }
            else
                return false;
        }

        private static void CacheImageResourceAsFile(string resource_name, Image obj)
        {
            if (!option.EnableFileCache || temporary_folder_path==null)
                return;

            Stream stream = null;

            //resource_name = FileNameHelper.FilterFileName(resource_name);
            resource_name = resource_name.EndsWith(".cache") ? resource_name : (resource_name + ".cache");
            var file_path = Path.Combine(temporary_folder_path, resource_name);

            try
            {
                if (!CheckAndDeleteCacheFile(obj, out stream))
                    return;

                var file_stream = File.OpenWrite(file_path);

                var buffer = new byte[1024];
                int read = 0;

                do
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    file_stream.Write(buffer, 0, read);
                } while (read != 0);

                Log.Debug($"Saved cache file :{file_path}");
            }
            catch (Exception e)
            {
                Log.Debug("Failed to cache image to cache folder:" + e.Message);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private static bool CheckAndDeleteCacheFile(Image obj, out Stream stream)
        {
            stream = new MemoryStream();

            try
            {
                obj.Save(stream, obj.RawFormat);
                var image_size = stream.Length;
                stream.Seek(0, SeekOrigin.Begin);

                var limit_size = option.CacheFolderMaxSize * 1024 * 1024; //MB -> bytes

                if (limit_size == 0 || current_record_capacity + image_size < limit_size)
                    return true;

                Log.Debug($"{current_record_capacity} + {image_size} >= {limit_size} ,delete old cache files");

                var delete_queue = Directory.EnumerateFiles(temporary_folder_path, "*.cache").Select(x =>
                {
                    using var stream = File.OpenRead(x);
                    return (File.GetLastAccessTime(x), stream.Length, x);
                }).OrderBy(x => x.Item1).ToArray();

                foreach (var (date, size, file_path) in delete_queue)
                {
                    File.Delete(file_path);
                    current_record_capacity -= size;

                    Log.Debug($"Delete old cache file for release folder space:({date},{size},{file_path}) , current_record_capacity={current_record_capacity}");

                    if (current_record_capacity + image_size < limit_size)
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                stream?.Dispose();
                throw e;
            }
        }

        #endregion
    }
}
