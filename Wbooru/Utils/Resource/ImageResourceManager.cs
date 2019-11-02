using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
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
    [Export(typeof(ImageResourceManager))]
    public class ImageResourceManager
    {
        private ObjectCache cache = MemoryCache.Default;
        private readonly string TEMP_PART = Path.GetTempPath();

        GlobalSetting setting;
        
        [ImportingConstructor]
        public ImageResourceManager([Import(typeof(SettingManager))]SettingManager manager)
        {
            setting = manager.LoadSetting<GlobalSetting>();
        }

        public Image RequestImage(string resource_name,Func<Image> manual_request)
        {
            if (TryGetImageFromMemoryCache(resource_name,out var res))
                return res;

            if (TryGetImageFromDownloadFolder(resource_name, out res))
                return res;

            if (manual_request() is Image obj)
            {
                cache[resource_name] = obj;
                return obj;
            }

            return null;
        }

        public Task<Image> RequestImageAsync(string resource_name,Func<Image> manual_request)
        {
            return Task.Run(()=> RequestImage(resource_name,manual_request));
        }

        private bool TryGetImageFromDownloadFolder(string name, out Image res)
        {
            res = null;

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            var file_path = Path.Combine(setting.DownloadPath, name);

            if (!File.Exists(file_path))
                return false;

            res = Image.FromFile(file_path);

            //todo
            return false;
        }

        private bool TryGetImageFromMemoryCache(string name, out Image res)
        {
            res = null;

            if (cache.Contains(name))
                return (res=cache[name] as Image) !=null;

            return false;
        }
    }
}
