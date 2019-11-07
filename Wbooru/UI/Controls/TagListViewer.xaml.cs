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
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.UI.Pages;
using Wbooru.Utils;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// TagListViewer.xaml 的交互逻辑
    /// </summary>
    public partial class TagListViewer : UserControl
    {
        public event Action<TagListViewer> CloseTagPanelEvent;

        public bool TagsListType
        {
            get { return (bool)GetValue(TagsListTypeProperty); }
            set { SetValue(TagsListTypeProperty, value); }
        }

        public static readonly DependencyProperty TagsListTypeProperty =
            DependencyProperty.Register("TagsListType", typeof(bool), typeof(TagListViewer), new PropertyMetadata(false,(e,d) => {
                ((TagListViewer)(e)).TagsList = (bool)d.NewValue ? TagManager.FiltedTags : TagManager.MarkedTags;
            }
        ));

        public ObservableCollection<Tag> TagsList
        {
            get { return (ObservableCollection<Tag>)GetValue(TagsListProperty); }
            private set { SetValue(TagsListProperty, value); }
        }

        public static readonly DependencyProperty TagsListProperty =
            DependencyProperty.Register("TagsList", typeof(ObservableCollection<Tag>), typeof(TagListViewer), 
                new PropertyMetadata(TagManager.MarkedTags));

        public TagListViewer()
        {
            InitializeComponent();

            RightTagsPanel.DataContext = this;
        }

        private void SearchCheckedTagsButton_Click(object sender, RoutedEventArgs e)
        {
            var tags = TagViewList.ItemContainerGenerator.Items
                .Select(x => TagViewList.ItemContainerGenerator.ContainerFromItem(x))
                .OfType<FrameworkElement>()
                .Select(x => ViusalTreeHelperEx.FindName("SelectCheckBox", x))
                .OfType<CheckBox>()
                .Where(x => x.IsChecked ?? false)
                .Select(x => x.DataContext)
                .OfType<Tag>()
                .Select(x => x.Name).ToArray();

            NavigationHelper.NavigationPush(new MainGalleryPage(tags));
        }

        private void CloseTagPanelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseTagPanelEvent?.Invoke(this);
        }

        private void SwitchTagListButton_Click(object sender, RoutedEventArgs e)
        {
            TagsListType = !TagsListType;
        }
    }
}
