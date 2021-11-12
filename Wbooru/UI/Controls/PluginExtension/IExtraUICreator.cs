using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wbooru.Kernel.DI;

namespace Wbooru.UI.Controls.PluginExtension
{
    public interface IExtraUICreator : IMultiImplementInjectable
    {
        UIElement Create();
    }
}
