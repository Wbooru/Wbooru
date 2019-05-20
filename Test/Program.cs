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
using Wbooru.Models.Gallery;
using Wbooru.Utils;

namespace Test
{
    class Program
    {
        static IEnumerable<int> F()
        {
            int i = 0;

            while (true)
            {
                for (int x = 0; x < 20; x++)
                {
                    yield return i++;
                }
            }
        }

        static void Main(string[] args)
        {
            Container.BuildDefault();

            var gallery = Container.Default.GetExportedValue<Gallery>();
            var manager = Container.Default.GetExportedValue<SchedulerManager>();
            var resource = Container.Default.GetExportedValue<ImageResourceManager>();

            var c = gallery.GetMainPostedImages().MakeMultiThreadable();

            List<GalleryItem> s = new List<GalleryItem>();

            for (int i = 0; i < 5; i++)
            {
                s.AddRange(Task.Run(() => c.Skip(s.Count).Take(20).ToArray()).Result);
                s.AddRange(Task.Run(() => c.Skip(s.Count).Take(20).ToArray()).Result);
                s.AddRange(Task.Run(() => c.Skip(s.Count).Take(20).ToArray()).Result);
            }
        }
    }
}
