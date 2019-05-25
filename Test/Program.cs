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
        static void Main(string[] args)
        {
            Container.BuildDefault();

            var formatter = Container.Default.GetExportedValue<CalculatableFormatter>();
            var x = formatter.FormatCalculatableString("0,40,50,0,0");

            Console.ReadLine();
        }
    }
}
