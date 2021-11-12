using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Wbooru.UI.Dialogs
{
    /// <summary>
    /// SettingRestartComfirmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SettingRestartComfirmDialog : DialogContentBase
    {
        public SettingRestartComfirmDialog()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Setting<GlobalSetting>.Current.IgnoreSettingChangedComfirm = (sender as CheckBox).IsChecked ?? false;
        }

        private void RestartComfirmButton_Click(object sender, RoutedEventArgs e)
        {
            App.UnusualSafeRestart();
        }

        private void NotRestartComfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Dialog.CloseDialog(this);
        }
    }
}
