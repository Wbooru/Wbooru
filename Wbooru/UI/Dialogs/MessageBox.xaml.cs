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
    /// MessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBox : DialogContentBase
    {
        public bool Result { get; set; }

        public bool ComfirmMessage { get; set; }

        public MessageBox(string title, string content) : this(title, content,"","")
        {
            YesButton.Visibility = NoButton.Visibility = Visibility.Hidden;
            ConfirmButton.Visibility = Visibility.Visible;
        }

        public MessageBox(string title,string content,string yes,string no)
        {
            InitializeComponent();

            YesButton.Content = yes;
            NoButton.Content = no;
            MainContent.Text = content;
            DialogTitle = title;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Dialog.CloseDialog(this);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Dialog.CloseDialog(this);
        }
    }
}
