using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wbooru.UI.Controls.PluginExtension
{
    public interface IExtraUICreator
    {
        UIElement Create();
    }
}
