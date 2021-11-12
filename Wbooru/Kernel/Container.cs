using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Wbooru.Kernel.DI;

//make get this anywhere
namespace Wbooru
{
    public static class Container
    {
        private static AggregateCatalog catalog;
        private static CompositionContainer instance = null;
        private static Dictionary<Type, object> cachedGotObjects = new Dictionary<Type, object>();

        public static void BuildDefault()
        {
            Directory.CreateDirectory("Plugins");

            var plugin_folders = Directory.GetDirectories("Plugins").Select(x => new DirectoryCatalog(x)).OfType<ComposablePartCatalog>();

            catalog = new AggregateCatalog(
                plugin_folders.Concat(new[] {
                    new AssemblyCatalog(typeof(Container).Assembly)
                }
               ));

            instance = new CompositionContainer(catalog);
            cachedGotObjects.Clear();

            foreach (var folder in plugin_folders)
                Log.Info($"Add folder for loading plugins : {folder}");
        }

        public static IEnumerable<T> GetAll<T>() => instance.GetExportedValues<T>();

        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (cachedGotObjects.TryGetValue(type, out var d))
                return d as T;

            var pickList = instance.GetExports<T, IPriorityMetadata>()
                .Select(x => (x, x.Metadata.Priority))
                .Concat(instance.GetExports<T>().Select(y => (new Lazy<T, IPriorityMetadata>(() => y.Value, null), (uint)0)))
                .GroupBy(x => x.Item2)
                .OrderByDescending(x => x.Key);

            var priorityGroup = pickList.FirstOrDefault().ToArray();
            var picked = priorityGroup.FirstOrDefault();
            var actualValue = picked.Item1.Value;
            Log.Debug(() => $"Require : {typeof(T).Name} , Pick : {actualValue.GetType().Name} , Priority : {picked.Item2} {(priorityGroup.Length > 1 ? ($"(More exports has same priority, please check it.)") : string.Empty)}");
            if (!(actualValue.GetType().GetCustomAttribute<PartCreationPolicyAttribute>() is PartCreationPolicyAttribute a && a.CreationPolicy == CreationPolicy.NonShared))
                cachedGotObjects[type] = actualValue;
            return actualValue;
        }
    }
}
