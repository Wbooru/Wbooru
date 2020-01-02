using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wbooru.Settings.UIAttributes;
using Wbooru.UI.Controls;

namespace Wbooru.Settings
{
    /// <summary>
    /// 表面SettingBase,实际提供简单的UI界面来执行各种命令
    /// </summary>
    [Export(typeof(SettingBase))]
    internal class DataOperationPanel : SettingBase
    {
        [CustomUI]
        public UIElement CreateEmbeddedDataOperationPanel() => new EmbeddedDataOperationPanel();
    }
}
