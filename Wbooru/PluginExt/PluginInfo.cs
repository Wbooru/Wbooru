using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;

namespace Wbooru.PluginExt
{
    public abstract class PluginInfo : IMultiImplementProvidable
    {
        public abstract string PluginName { get; }
        public Version PluginVersion => (this is IPluginUpdatable updatable) ? updatable.CurrentPluginVersion : GetType().Assembly.GetName().Version;
        public abstract string PluginProjectWebsite { get; }
        public abstract string PluginAuthor { get; }
        public abstract string PluginDescription { get; }

        protected virtual void OnApplicationTerm()
        {

        }

        internal void CallApplicationTerm() => OnApplicationTerm();
    }
}
