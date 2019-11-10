using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Models.Gallery;
using Wbooru.Settings;
using Wbooru.Kernel;
using Wbooru.Galleries;
using static Wbooru.UI.Pages.MainGalleryPage;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// GalleryGridView.xaml 的交互逻辑
    /// </summary>
    public partial class GalleryGridView : UserControl
    {
        public event Action<GalleryItem> ClickItemEvent;
        public event Action<GalleryGridView> OnRequestMoreItemStarted;
        public event Action<GalleryGridView> OnRequestMoreItemFinished;

        /// <summary>
        /// 表示每张图片固定宽度
        /// </summary>
        public uint GridItemWidth
        {
            get { return (uint)GetValue(GridItemWidthProperty); }
            set { SetValue(GridItemWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemWidthProperty =
            DependencyProperty.Register("GridItemWidth", typeof(uint), typeof(GalleryGridView), new PropertyMetadata((uint)150));

        /// <summary>
        /// 表示每张图片之间的间隔
        /// </summary>
        public uint GridItemMarginWidth
        {
            get { return (uint)GetValue(GridItemMarginWidthProperty); }
            set { SetValue(GridItemMarginWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemMarginWidthProperty =
            DependencyProperty.Register("GridItemMarginWidth", typeof(uint), typeof(GalleryGridView), new PropertyMetadata((uint)10));

        public Gallery Gallery
        {
            get { return (Gallery)GetValue(GalleryProperty); }
            set { SetValue(GalleryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(Gallery), typeof(GalleryGridView), new PropertyMetadata(null));

        /// <summary>
        /// 表示可以加载的列表源，一般View会自动遍历此集合来拿更多的图片数据.
        /// </summary>
        public IEnumerable<GalleryItem> LoadableSource
        {
            get { return (IEnumerable<GalleryItem>)GetValue(LoadableSourceProperty); }
            set { SetValue(LoadableSourceProperty, value); }
        }

        public static readonly DependencyProperty LoadableSourceProperty =
            DependencyProperty.Register("LoadableSource", typeof(IEnumerable<GalleryItem>), typeof(GalleryGridView), new PropertyMetadata((e, d) => ((GalleryGridView)e).OnLoadableSourceChanged()));

        /// <summary>
        /// 设置此控件的用处，一般用于过滤指定标签的图片列表
        /// </summary>
        public GalleryViewType ViewType { get; set; }

        private ObservableCollection<GalleryItem> Items = new ObservableCollection<GalleryItem>();

        public GalleryGridView()
        {
            InitializeComponent();

            DataContext = this;

            GalleryList.ItemsSource = Items;

            UpdateSettingForScroller();
        }

        private void OnLoadableSourceChanged()
        {
            Items.Clear();

            TryRequestMoreItemFromLoadableSource();
        }

        public void UpdateSettingForScroller()
        {
            var scrollbar_visiable = SettingManager.LoadSetting<GlobalSetting>().GalleryListScrollBarVisiable;

            if (scrollbar_visiable)
            {
                ListScrollViewer.MouseMove -= ListScrollViewer_PreviewMouseMove;
                ListScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                ListScrollViewer.MouseMove += ListScrollViewer_PreviewMouseMove;
                ListScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
        }

        private void ListScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var height = (sender as ScrollViewer).ScrollableHeight;
            var at_end = e.VerticalOffset
                                    >= height;

            if (at_end)
                TryRequestMoreItemFromLoadableSource();
        }

        public void ClearGallery()
        {
            Items.Clear();
            Gallery = null;
            LoadableSource = null;
        }

        private async void TryRequestMoreItemFromLoadableSource()
        {
            if (is_requesting)
            {
                Log.Debug("Task is running");
                return;
            }

            if (LoadableSource == null)
                return;

            OnRequestMoreItemStarted?.Invoke(this);

            var option = SettingManager.LoadSetting<GlobalSetting>();

            is_requesting = true;

            var count = Items.Count;
            Gallery gallery = Gallery;
            IEnumerable<GalleryItem> source = LoadableSource; 

            var list = await Task.Run(() => FilterTag(source.Skip(count), gallery).Take(option.GetPictureCountPerLoad).ToArray());

            Log.Debug($"Skip({count}) Take({option.GetPictureCountPerLoad}) ActualTake({list.Count()})", "GridViewer_RequestMoreItems");

            Dispatcher.Invoke(new Action(() =>
            {
                if (source != LoadableSource)
                    return;

                foreach (var item in list)
                    Items.Add(item);

                if (list.Count() < option.GetPictureCountPerLoad)
                    Toast.ShowMessage("已到达图片队列末尾");
            }));

            OnRequestMoreItemFinished?.Invoke(this);
            is_requesting = false;
        }

        public IEnumerable<GalleryItem> FilterTag(IEnumerable<GalleryItem> items, Gallery gallery=null)
        {
            var option = SettingManager.LoadSetting<GlobalSetting>();
            IEnumerable<Gallery> galleries = gallery == null ? Container.Default.GetExportedValues<Gallery>() : new[] { gallery };

            return items.Where(x =>
            {
                if (galleries.Select(g=>(g.GetImageDetial(x),g)).FirstOrDefault() is (GalleryImageDetail detail,Gallery gallery))
                {
                    if (!option.EnableTagFilter)
                        return true;

                    switch (ViewType)
                    {
                        case GalleryViewType.Marked:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.MarkedWindow))
                                return true;
                            break;
                        case GalleryViewType.Voted:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.VotedWindow))
                                return true;
                            break;
                        case GalleryViewType.Main:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.MainWindow))
                                return true;
                            break;
                        case GalleryViewType.SearchResult:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.SearchResultWindow))
                                return true;
                            break;
                        default:
                            break;
                    }

                    var filter_list = option.UseAllGalleryFilterList ? TagManager.FiltedTags : TagManager.FiltedTags.Where(x => x.FromGallery == gallery.GalleryName);

                    foreach (var filter_tag in filter_list)
                    {
                        if (detail.Tags.FirstOrDefault(x => x == filter_tag.Tag.Name) is string captured_filter_tag)
                        {
                            Log.Debug($"Skip this item because of filter:{captured_filter_tag} -> {string.Join(" ", detail.Tags)}");
                            return false;
                        }
                    }
                }

                return true;
            });
        }

        private void ListScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            drag_action_state = DragActionState.Idle;
            Log.Debug($"{drag_action_state}", "ListScrollViewer_MouseLeave");
        }

        private void ListScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (drag_action_state == DragActionState.ReadyDrag)
            {
                drag_action_state = DragActionState.Dragging;
                Log.Debug($"{drag_action_state}", "ListScrollViewer_PreviewMouseMove");
            }

            if (DragActionState.Dragging == drag_action_state)
            {
                var y = e.GetPosition(this).Y;
                var offset = prev_y - y;
                prev_y = y;

                ListScrollViewer.ScrollToVerticalOffset(ListScrollViewer.VerticalOffset + offset);
                Log.Debug($"Moving ... {drag_action_state}", "ListScrollViewer_PreviewMouseMove");
            }
        }

        DragActionState drag_action_state = DragActionState.Idle;

        private enum DragActionState
        {
            Idle,
            ReadyDrag,
            Dragging
        }

        double prev_y = 0;
        private bool is_requesting;

        private void ListScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            drag_action_state = DragActionState.Idle;
            Log.Debug($"{drag_action_state}", "ListScrollViewer_MouseUp");
        }

        private void ListScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prev_y = e.GetPosition(this).Y;
            drag_action_state = DragActionState.ReadyDrag;

            Log.Debug($"{drag_action_state}", "ListScrollViewer_PreviewMouseLeftButtonDown");
        }

        private void StackPanel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (drag_action_state != DragActionState.ReadyDrag || !(((FrameworkElement)sender).DataContext is GalleryItem item))
                return;

            Log.Debug($"click item {item.GalleryItemID}");
            ClickItemEvent?.Invoke(item);
        }
    }
}
