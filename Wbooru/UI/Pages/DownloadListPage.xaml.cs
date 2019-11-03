using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Wbooru.Models;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// DownloadListPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadListPage : Page
    {
        public ObservableCollection<Download> DownloadList = new ObservableCollection<Download>();

        public DownloadListPage()
        {
            InitializeComponent();

            MainPanel.DataContext = this;

            
        }
    }
}
