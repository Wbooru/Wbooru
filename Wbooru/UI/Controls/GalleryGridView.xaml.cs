﻿using System;
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
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Utils;
using System.Threading;
using Wbooru.Utils.Resource;

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
        public Func<IAsyncEnumerable<GalleryItem>> LoadableSource
        {
            get { return (Func<IAsyncEnumerable<GalleryItem>>)GetValue(LoadableSourceProperty); }
            set { SetValue(LoadableSourceProperty, value); }
        }

        public static readonly DependencyProperty LoadableSourceProperty =
            DependencyProperty.Register("LoadableSource", typeof(Func<IAsyncEnumerable<GalleryItem>>), typeof(GalleryGridView), new PropertyMetadata((e, d) => ((GalleryGridView)e).OnLoadableSourceChanged()));

        /// <summary>
        /// 设置此控件的用处，一般用于过滤指定标签的图片列表
        /// </summary>
        public GalleryViewType ViewType { get; set; }

        public ObservableCollection<GalleryItem> Items { get; } = new ObservableCollection<GalleryItem>();

        private int current_index = 0;

        public int DisplayedLogicPageIndex => current_index / Setting<GlobalSetting>.Current.GetPictureCountPerLoad;

        public GalleryGridView()
        {
            InitializeComponent();

            DataContext = this;

            GalleryList.ItemsSource = Items;

            UpdateSettingForScroller();
        }

        private IAsyncEnumerator<IEnumerable<GalleryItem>> loadable_items;
        private SkipCounterWrapper skipCount;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void OnLoadableSourceChanged(int specifyIndex = 0)
        {
            cancellationTokenSource?.Cancel();
            CleanCurrentItems();
            ListScrollViewer.ScrollToVerticalOffset(0);
            current_index = specifyIndex;

            skipCount = new SkipCounterWrapper();
            cancellationTokenSource = new CancellationTokenSource();

            loadable_items = FilterTag(LoadableSource?.Invoke() ?? AsyncEnumerable.Empty<GalleryItem>(), skipCount, Gallery).Where(x =>
           {
               if (unique_items.Contains(x.GalleryItemID))
                   return false;
               unique_items.Add(x.GalleryItemID);
               return true;
           }).Skip(specifyIndex).SequenceWrapAsync(Setting<GlobalSetting>.Current.GetPictureCountPerLoad).GetAsyncEnumerator();

            Log.Debug($"generate new loadable_items({loadable_items?.GetHashCode()})");

            TryRequestMoreItemFromLoadableSource();
        }

        public void UpdateSettingForScroller()
        {
            var scrollbar_visiable = Setting<GlobalSetting>.Current.GalleryListScrollBarVisiable;

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
            var at_end = e.VerticalOffset >= height;

            if (at_end)
                TryRequestMoreItemFromLoadableSource();
        }

        public void ClearGallery()
        {
            CleanCurrentItems();
            Gallery = null;
            LoadableSource = null;
        }

        private async void TryRequestMoreItemFromLoadableSource(CancellationToken? cancellation = default)
        {
            var cancellationToken = cancellation ?? cancellationTokenSource?.Token ?? default;

            if (is_requesting)
            {
                Log.Debug("Task is running");
                return;
            }

            if (loadable_items is null || LoadableSource is null)
                return;

            OnRequestMoreItemStarted?.Invoke(this);

            is_requesting = true;

            var source = loadable_items;
            var perLoad = Setting<GlobalSetting>.Current.GetPictureCountPerLoad;

            Log.Debug($"开始尝试获取列表({source.GetHashCode()})");

            var list = Enumerable.Empty<GalleryItem>();

            try
            {
                if (await loadable_items.MoveNextAsync())
                {
                    list = loadable_items.Current ?? Enumerable.Empty<GalleryItem>();
                }
            }
            catch (Exception e)
            {
                Toast.ShowMessage($"无法获取图片列表数据:{e.Message}");
                return;
            }

            Log.Debug($"尝试获取列表结束({source.GetHashCode()} - {loadable_items?.GetHashCode()})");

            if (source == loadable_items && !cancellationToken.IsCancellationRequested)
            {
                var c = 0;
                foreach (var item in list)
                {
                    c++;
                    Items.Add(item);
                }

                Log.Debug($"Skip({current_index}) Filter({skipCount.Count}) Take({perLoad}) ActualTake({c})", "GridViewer_RequestMoreItems");

                if (c < perLoad)
                    Toast.ShowMessage("已到达图片队列末尾");
                current_index += c + skipCount.Count;
            }

            if (cancellationToken.IsCancellationRequested)
                Log.Debug($"拿到的({source.GetHashCode()} - {loadable_items?.GetHashCode()})抛弃，因为已被要求取消");

            OnRequestMoreItemFinished?.Invoke(this);
            is_requesting = false;

            if (source != loadable_items && loadable_items != null)
                TryRequestMoreItemFromLoadableSource();
        }

        public class SkipCounterWrapper
        {
            public int Count { get; set; } = 0;
        }

        public IAsyncEnumerable<GalleryItem> FilterTag(IAsyncEnumerable<GalleryItem> items, SkipCounterWrapper counter, Gallery gallery = null)
        {
            var option = Setting<GlobalSetting>.Current;
            var galleries = gallery == null ? Container.GetAll<Gallery>() : new[] { gallery };

            return items.PartParallelableSelectAsync(async x =>
            {
                if (galleries.FirstOrDefault(g => g.GalleryName == x.GalleryName) is Gallery gallery && (await gallery.GetImageDetial(x)) is GalleryImageDetail detail)
                {
                    if (!option.EnableTagFilter)
                        return x;

                    //todo: optimze them
                    switch (ViewType)
                    {
                        case GalleryViewType.Marked:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.MarkedWindow))
                                return x;
                            break;
                        case GalleryViewType.Voted:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.VotedWindow))
                                return x;
                            break;
                        case GalleryViewType.Main:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.MainWindow))
                                return x;
                            break;
                        case GalleryViewType.SearchResult:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.SearchResultWindow))
                                return x;
                            break;
                        case GalleryViewType.History:
                            if (!option.FilterTarget.HasFlag(TagFilterTarget.HistoryWindow))
                                return x;
                            break;
                        default:
                            break;
                    }

                    var filter_list = option.UseAllGalleryFilterList ? Container.Get<ITagManager>().FiltedTags : Container.Get<ITagManager>().FiltedTags.Where(x => x.FromGallery == gallery.GalleryName);

                    foreach (var filter_tag in filter_list)
                    {
                        if (detail.Tags.FirstOrDefault(x => x == filter_tag.Tag.Name) is string captured_filter_tag)
                        {
                            Log.Debug($"Skip this item because of filter:{captured_filter_tag} -> {string.Join(" ", detail.Tags)}");
                            counter.Count++;
                            return null;
                        }
                    }
                }

                return x;
            }).OfType<GalleryItem>();
        }

        private void ListScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            drag_action_state = DragActionState.Idle;
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

        HashSet<string> unique_items = new HashSet<string>();

        private void CleanCurrentItems()
        {
            Items.Clear();
            unique_items.Clear();
        }

        public void ChangePage(int page)
        {
            /*
             * 如果Gallery支持IGalleryItemIteratorFastSkipable,且此插件是用于主页面显示的，则使用此接口的功能
             */
            if (ViewType != GalleryViewType.Main || !(Gallery is IGalleryItemIteratorFastSkipable feature))
            {
                Log.Info($"Use default method to skip items.({ViewType} - {Gallery?.GalleryName} - {Gallery is IGalleryItemIteratorFastSkipable})");
                CleanCurrentItems();
                current_index = page * Setting<GlobalSetting>.Current.GetPictureCountPerLoad;
                OnLoadableSourceChanged(current_index);
            }
            else
            {
                Log.Info($"Use IGalleryItemIteratorFastSkipable.IteratorSkip() to skip items.({ViewType} - {Gallery.GalleryName} - {Gallery is IGalleryItemIteratorFastSkipable})");

                //这里不会根据因刷新而开头会有不同的变化
                var list = feature.IteratorSkipAsync(page * Setting<GlobalSetting>.Current.GetPictureCountPerLoad);
                LoadableSource = new Func<IAsyncEnumerable<GalleryItem>>(() => list);
            }
        }

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

        internal void RefreshItem()
        {
            Log.Info("refresh gallery.");
            OnLoadableSourceChanged();
        }

        CancellationTokenSource copyTask = new CancellationTokenSource();

        private void OnCopyLink(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement f && f.DataContext is GalleryItem item))
                return;
            copyTask?.Cancel();
            copyTask = new CancellationTokenSource();

            Toast.ShowMessage("复制成功");
            Clipboard.SetText(item?.DetailLink);
        }

        private async void OnCopyPic(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement f && f.DataContext is GalleryItem item))
                return;
            copyTask?.Cancel();
            copyTask = new CancellationTokenSource();

            var gallery = Gallery;
            var detial = await gallery.GetImageDetial(item);
            var dl = detial.PickSuitableImageURL(Setting<GlobalSetting>.Current.SelectPreferViewQualityTarget);

            Toast.ShowMessage("开始加载图片...");
            using var image = await ImageResourceManager.RequestImageAsync(dl.FullFileName, dl.DownloadLink, true, default, default, copyTask.Token);
            if (copyTask.Token.IsCancellationRequested)
                return;

            Clipboard.SetImage(image.ConvertToBitmapImage());
            if (copyTask.Token.IsCancellationRequested)
                return;
            Toast.ShowMessage("复制图片成功");
        }

        private async void OnCopyID(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement f && f.DataContext is GalleryItem item))
                return;
            copyTask?.Cancel();
            copyTask = new CancellationTokenSource();

            var gallery = Gallery;
            var detial = await gallery.GetImageDetial(item);

            Toast.ShowMessage("复制成功");
            Clipboard.SetText(detial?.ID);
        }
    }
}
