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
using Wbooru.Models.Gallery;

namespace Wbooru.UI.Dialogs
{
    /// <summary>
    /// SelectableImageList.xaml 的交互逻辑
    /// </summary>
    public partial class SelectableImageList : UserControl
    {
        public IEnumerable<DownloadableImageLink> List { get; set; }
        public DownloadableImageLink CurrentDisplayImageLink { get; set; }

        public SelectableImageList(IEnumerable<(int index, DownloadableImageLink item)> list, DownloadableImageLink currentDisplayImageLink)
        {
            List = list.Select(x=>x.item);
            CurrentDisplayImageLink = currentDisplayImageLink;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Dialog.CloseDialog(this);

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CurrentDisplayImageLink = SelectList.SelectedItem as DownloadableImageLink;

            Dialog.CloseDialog(this);
        }
    }
}
