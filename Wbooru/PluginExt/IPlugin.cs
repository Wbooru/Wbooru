using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.PluginExt
{
    public interface IPlugin
    {
        string PluginName { get; }
        string PluginAuthor { get; }
    }
}
