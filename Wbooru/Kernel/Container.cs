using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wbooru
{
    public static class Container
    {
        private static CompositionContainer instance = null;

        public static CompositionContainer Default
        {
            get
            {
                if (instance != null)
                    return instance;

                throw new Exception("MEF hasn't been initalized yet.");
            }
        }

        public static void BuildDefault()
        {
            Directory.CreateDirectory("Plugins");

            var plugin_folders = Directory.GetDirectories("Plugins").Select(x => new DirectoryCatalog(x)).OfType<ComposablePartCatalog>();

            var catalog = new AggregateCatalog(
                plugin_folders.Concat(new[] {
                    new AssemblyCatalog(typeof(Container).Assembly)
                }
               ));

            instance = new CompositionContainer(catalog);
        }
    }
}
