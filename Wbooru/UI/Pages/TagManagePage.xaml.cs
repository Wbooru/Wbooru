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
using Wbooru.Galleries;
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.UI.Controls;
using Wbooru.Utils;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// TagManagePage.xaml 的交互逻辑
    /// </summary>
    public partial class TagManagePage : Page
    {
        public CollectionViewSource MarkedTagsView { get; } = new CollectionViewSource();
        public CollectionViewSource FilterTagsView { get; } = new CollectionViewSource();
        public CollectionViewSource SubscribedTagsView { get; } = new CollectionViewSource();

        public TagManagePage()
        {
            InitializeComponent();

            var list = Container.GetAll<Gallery>().Select(x => x.GalleryName).ToList();
            list.Insert(0, "AllGallery");

            GalleriesSelector.ItemsSource = list;
            GalleriesSelector.SelectedIndex = 0;

            MarkedTagsView.Source = Container.Get<ITagManager>().MarkedTags;
            MarkedTagsView.Filter += MarkedTagsView_Filter;

            FilterTagsView.Source = Container.Get<ITagManager>().FiltedTags;
            FilterTagsView.Filter += FilterTagsView_Filter;

            SubscribedTagsView.Source = Container.Get<ITagManager>().SubscribedTags;
            SubscribedTagsView.Filter += SubscribedTagsView_Filter;

            MainContent.DataContext = this;
        }

        private void SubscribedTagsView_Filter(object sender, FilterEventArgs e)
        {
            Common_Filter(sender, e);
            if (!e.Accepted)
                return;
        }

        private void FilterTagsView_Filter(object sender, FilterEventArgs e)
        {
            Common_Filter(sender, e);
            if (!e.Accepted)
                return;
        }

        private void MarkedTagsView_Filter(object sender, FilterEventArgs e)
        {
            Common_Filter(sender, e);
            if (!e.Accepted)
                return;
        }

        private void Common_Filter(object sender, FilterEventArgs e)
        {
            if (!(e.Item is TagRecord records))
            {
                e.Accepted = false;
                return;
            }

            var gallery_name = GalleriesSelector.SelectedItem as string;

            if (gallery_name != "AllGallery" && gallery_name != records.FromGallery)
            {
                e.Accepted = false;
                return;
            }

            e.Accepted = true;
        }

        private void CloseTagPanelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void GalleriesSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MarkedTagsView?.View?.Refresh();
            FilterTagsView?.View?.Refresh();
            SubscribedTagsView?.View?.Refresh();
        }

        private async void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            var record = (sender as FrameworkElement).DataContext as TagRecord;

            if (record.RecordType == TagRecord.TagRecordType.Subscribed)
            {
                await Container.Get<ITagManager>().UnSubscribedTag(record);
                Toast.ShowMessage("已取消订阅此标签");
            }
            else
            {
                await Container.Get<ITagManager>().RemoveTag(record);
                Toast.ShowMessage("已删除此标签");
            }
        }

        private void SearchCheckedTagsButton_Click(object sender, RoutedEventArgs e)
        {
            var tags = MarkedTagList.ItemContainerGenerator.Items
                .Select(x => MarkedTagList.ItemContainerGenerator.ContainerFromItem(x))
                .OfType<FrameworkElement>()
                .Select(x => VisualTreeHelperEx.FindName("SelectCheckBox", x))
                .OfType<CheckBox>()
                .Where(x => x.IsChecked ?? false)
                .Select(x => x.DataContext)
                .OfType<TagRecord>()
                .Select(x => x.Tag.Name).ToArray();

            if (!tags.Any())
            {
                Toast.ShowMessage("请至少选择一项标签");
                return;
            }

            var gallery = Container.GetAll<Gallery>().Where(x => GalleriesSelector.SelectedItem.ToString() == x.GalleryName).FirstOrDefault();

            if (gallery == null)
            {
                Toast.ShowMessage("请选择有效的画廊");
                return;
            }

            NavigationHelper.NavigationPush(new MainGalleryPage(tags, gallery));
        }

        private async void SubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            var record = (sender as FrameworkElement).DataContext as TagRecord;

            await Container.Get<ITagManager>().SubscribedTag(record);

            Toast.ShowMessage("成功订阅此标签");
        }
    }
}
