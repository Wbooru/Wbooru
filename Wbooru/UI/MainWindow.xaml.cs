using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using Wbooru.Kernel;
using Wbooru.Settings;
using Wbooru.UI.Pages;
using static Wbooru.UI.Controls.Toast;

namespace Wbooru.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Container.Default.ComposeExportedValue(WindowFrame);
            Container.Default.ComposeExportedValue(MainToast);

            var navigation = Container.Default.GetExportedValue<NavigationHelper>();
            navigation.NavigationPush(new MainGalleryPage());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            DownloadManager.Close();
            Container.Default.GetExportedValue<SettingManager>().SaveSettingFile();
        }
    }
}
