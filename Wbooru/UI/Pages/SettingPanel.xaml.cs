using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// SettingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPanel : Page
    {
        public IEnumerable<SettingBase> SupportSettings
        {
            get { return (IEnumerable<SettingBase>)GetValue(SupportSettingsProperty); }
            set { SetValue(SupportSettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SupportSettingsProperty =
            DependencyProperty.Register("SupportSettings", typeof(IEnumerable<SettingBase>), typeof(SettingPanel), new PropertyMetadata(Enumerable.Empty<IUIVisualizable>()));

        public SettingPanel()
        {
            InitializeComponent();

            SupportSettings = Container.Default.GetExports<IUIVisualizable>().Select(x=>x.Value).OfType<SettingBase>();
        }
    }
}
