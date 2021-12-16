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
    public class DialogContentBase : UserControl
    {
        public string DialogTitle
        {
            get { return (string)GetValue(DialogTitleProperty); }
            set { SetValue(DialogTitleProperty, value); }
        }

        public bool AllowImplictClose
        {
            get { return (bool)GetValue(AllowImplictCloseProperty); }
            set { SetValue(AllowImplictCloseProperty, value); }
        }

        public static readonly DependencyProperty AllowImplictCloseProperty =
            DependencyProperty.Register("AllowImplictClose", typeof(bool), typeof(DialogContentBase), new PropertyMetadata(true));

        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register("DialogTitle", typeof(string), typeof(DialogContentBase), new PropertyMetadata(null));

        public void CloseDialog() => Dialog.CloseDialog(this);
    }
}
