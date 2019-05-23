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

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// GalleryGridView.xaml 的交互逻辑
    /// </summary>
    public partial class GalleryGridView : UserControl
    {
        public event Action<GalleryGridView> RequestMoreItems;

        public uint GridItemWidth
        {
            get { return (uint)GetValue(GridItemWidthProperty); }
            set { SetValue(GridItemWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemWidthProperty =
            DependencyProperty.Register("GridItemWidth", typeof(uint), typeof(GalleryGridView), new PropertyMetadata((uint)150));

        public uint GridItemMarginWidth
        {
            get { return (uint)GetValue(GridItemMarginWidthProperty); }
            set { SetValue(GridItemMarginWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemMarginWidthProperty =
            DependencyProperty.Register("GridItemMarginWidth", typeof(uint), typeof(GalleryGridView), new PropertyMetadata((uint)10));

        public GalleryItemUIElementWrapper GalleryItemsSource
        {
            get { return (GalleryItemUIElementWrapper)GetValue(GalleryItemsSourceProperty); }
            set { SetValue(GalleryItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty GalleryItemsSourceProperty =
            DependencyProperty.Register("GalleryItemsSource", typeof(GalleryItemUIElementWrapper), typeof(GalleryGridView), new PropertyMetadata(null));

        public GalleryGridView()
        {
            InitializeComponent();

            DataContext = this;

            ListScrollViewer.ScrollChanged += ListScrollViewer_ScrollChanged;
            ListScrollViewer.PreviewMouseLeftButtonUp += ListScrollViewer_PreviewMouseLeftButtonUp;
            ListScrollViewer.PreviewMouseLeftButtonDown += ListScrollViewer_PreviewMouseLeftButtonDown;
            ListScrollViewer.PreviewMouseMove += ListScrollViewer_PreviewMouseMove;
            ListScrollViewer.MouseLeave += ListScrollViewer_MouseLeave;
        }

        private void ListScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var height = (sender as ScrollViewer).ScrollableHeight;
            var at_end = e.VerticalOffset
                                    >= height;

            if (at_end /*&& height!=0*/)
                RequestMoreItems?.Invoke(this);
        }

        private void ListScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            is_drag = false;
        }

        private void ListScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (is_drag)
            {
                var y = e.GetPosition(this).Y;
                var offset = prev_y - y;
                prev_y = y;

                ListScrollViewer.ScrollToVerticalOffset(ListScrollViewer.VerticalOffset + offset);
            }
        }

        bool is_drag = false;
        double prev_y = 0;

        private void ListScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            is_drag = false;
            e.Handled = true;
        }

        private void ListScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prev_y = e.GetPosition(this).Y;
            is_drag = true;
            e.Handled = true;
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
