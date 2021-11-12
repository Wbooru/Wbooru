using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Wbooru.Kernel.DI;

namespace Wbooru
{
    public static class Container
    {
        private static AggregateCatalog catalog;
        private static CompositionContainer instance = null;

        private static Dictionary<Type, object> cachedGotObjects = new Dictionary<Type, object>();

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

            foreach (var folder in plugin_folders)
                Log.Info($"Add folder for loading plugins : {folder}");

            catalog = new AggregateCatalog(
                plugin_folders.Concat(new[] {
                    new AssemblyCatalog(typeof(Container).Assembly)
                }
               ));

            instance = new CompositionContainer(catalog);
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (cachedGotObjects.TryGetValue(type, out var d))
                return d as T;

            var pickList = Default.GetExports<T, IPriorityMetadata>()
                .Select(x => (x, x.Metadata.Priority))
                .Concat(Default.GetExports<T>().Select(y => (new Lazy<T, IPriorityMetadata>(() => y.Value, null), (uint)0)))
                .GroupBy(x => x.Item2)
                .OrderByDescending(x => x.Key)
                .ToArray();

            var picked = pickList.FirstOrDefault().FirstOrDefault();
            var actualValue = picked.Item1.Value;
            Log.Debug($"Require : {typeof(T).Name} , Pick : {actualValue.GetType().Name} , Priority : {picked.Item2}");
            if (!(actualValue.GetType().GetCustomAttribute<PartCreationPolicyAttribute>() is PartCreationPolicyAttribute a && a.CreationPolicy == CreationPolicy.NonShared))
                cachedGotObjects[type] = actualValue;
            return actualValue;
        }
    }
}
