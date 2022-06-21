using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wbooru.Network;
using Wbooru.PluginExt;
using Wbooru.Settings;
using Wbooru.UI.Controls;

namespace Wbooru.Utils.Resource
{
    public static class ImageResourceManager
    {
        private static GlobalSetting option;
        private static string temporary_folder_path;

        static ImageResourceManager()
        {
            option = Setting<GlobalSetting>.Current;

            if (option.EnableFileCache)
            {
                try
                {
                    temporary_folder_path = CacheFolderHelper.CacheFolderPath;

                    Directory.CreateDirectory(temporary_folder_path);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to check&create tempoary cache folder:" + e.Message);
                }
            }
        }

        public static async Task<Image> RequestImageAsync(string resource_name, string url, bool load_first, Action<(long downloaded_bytes, long content_bytes)> reporter = default, Action<HttpWebRequest> customReqFunc = default, CancellationToken cancellationToken = default)
        {
            const int retry = 3;

            for (int i = 0; i < retry; i++)
                if (await RequestImageAsync(resource_name, async () => await Container.Get<ImageFetchDownloadScheduler>().DownloadImageAsync(url, cancellationToken, reporter, customReqFunc, load_first)) is Image image)
                    return image;

            return default;
        }

        public static async Task<Image> RequestImageAsync(string resource_name, Func<Task<Image>> manual_request)
        {
            var hash = resource_name.CalculateMD5();
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

            var file_path = Path.Combine(Setting<GlobalSetting>.Current.DownloadPath, name);

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
            if (!option.EnableFileCache || temporary_folder_path == null)
                return;


            resource_name = resource_name.EndsWith(".cache") ? resource_name : (resource_name + ".cache");
            var file_path = Path.Combine(temporary_folder_path, resource_name);

            try
            {
                using var stream = new MemoryStream();
                obj.Save(stream, obj.RawFormat);
                stream.Seek(0, SeekOrigin.Begin);

                using var file_stream = File.OpenWrite(file_path);

                var buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
                int read = 0;

                do
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    file_stream.Write(buffer, 0, read);
                } while (read != 0);

                ArrayPool<byte>.Shared.Return(buffer);

                Log.Debug($"Saved cache file :{file_path}");
            }
            catch (Exception e)
            {
                Log.Debug("Failed to cache image to cache folder:" + e.Message);
            }
        }

        #endregion
    }
}
