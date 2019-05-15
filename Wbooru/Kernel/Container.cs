using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru
{
    public static class Container
    {
        public static CompositionContainer Default { get; private set; } = null;

        public static void BuildDefault()
        {
            if (!Directory.Exists("Plugins"))
                Directory.CreateDirectory("Plugins");

            var catalog = new AggregateCatalog(
                new DirectoryCatalog("Plugins"),
                new DirectoryCatalog("."),
                new AssemblyCatalog(typeof(Container).Assembly)
                );

            Default = new CompositionContainer(catalog);
        }
    }
}
