using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru;
using Wbooru.Galleries;
using YandeSourcePlugin;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Container.BuildDefault();
            var gallery=Container.Default.GetExportedValue<Gallery>();
             
            var result = gallery.SearchImages(new[] { "penis","stockings"}).FirstOrDefault();
        }
    }
}
