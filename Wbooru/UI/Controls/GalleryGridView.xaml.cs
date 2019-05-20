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

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// GalleryGridView.xaml 的交互逻辑
    /// </summary>
    public partial class GalleryGridView : UserControl
    {
        public uint GridItemWidth
        {
            get { return (uint)GetValue(GridItemWidthProperty); }
            set { SetValue(GridItemWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemWidthProperty =
            DependencyProperty.Register("GridItemWidth", typeof(uint), typeof(GalleryGridView), new PropertyMetadata(150));

        public int GridItemMarginWidth
        {
            get { return (int)GetValue(GridItemMarginWidthProperty); }
            set { SetValue(GridItemMarginWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemMarginWidthProperty =
            DependencyProperty.Register("GridItemMarginWidth", typeof(int), typeof(GalleryGridView), new PropertyMetadata(10));

        public GalleryItemUIElementWrapper ItemCollectionWrapper
        {
            get { return (GalleryItemUIElementWrapper)GetValue(ItemCollectionWrapperProperty); }
            set { SetValue(ItemCollectionWrapperProperty, value); }
        }

        public static readonly DependencyProperty ItemCollectionWrapperProperty =
            DependencyProperty.Register("ItemCollectionWrapper", typeof(GalleryItemUIElementWrapper), typeof(GalleryGridView), new PropertyMetadata(new GalleryItemUIElementWrapper()));



        public GalleryGridView()
        {
            InitializeComponent();

            DataContext = this;
        }
    }
}
