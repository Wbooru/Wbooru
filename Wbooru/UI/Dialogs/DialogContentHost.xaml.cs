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

namespace Wbooru.UI.Dialogs
{
    /// <summary>
    /// DialogContentHost.xaml 的交互逻辑
    /// </summary>
    public partial class DialogContentHost : UserControl
    {
        public DialogContentBase DialogContent => Content as DialogContentBase;

        public DialogContentHost()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DialogContent.AllowImplictClose)
                Dialog.CloseDialog(this);
        }

        private void ContentPresenter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DialogContent.AllowImplictClose)
                e.Handled = true;
        }
    }
}
