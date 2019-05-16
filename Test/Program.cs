using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru;
using Wbooru.Galleries;
using Wbooru.Kernel;
using Wbooru.Network;
using Wbooru.Utils.Resource;
using System.Windows;
using YandeSourcePlugin;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Container.BuildDefault();

            var gallery = Container.Default.GetExportedValue<Gallery>();
            var manager = Container.Default.GetExportedValue<SchedulerManager>();
            var resource = Container.Default.GetExportedValue<ImageResourceManager>();

            var result = gallery.GetMainPostedImages().FirstOrDefault() as IContainDetail;

            var download_link = result.GalleryDetail.DownloadableImageLinks.First().DownloadLink;
            var name = $"{result.GalleryDetail.ID} {string.Join(" ",result.GalleryDetail.Tags)}{Path.GetExtension(download_link)}";

            var image=resource.RequestImageAsync(name, () => {
                var downloader = Container.Default.GetExportedValue<ImageFetchDownloadSchedule>();
                return downloader.GetImageAsync(download_link);
            });
        }
    }
}
