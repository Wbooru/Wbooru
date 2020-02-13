using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
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
using Wbooru.UI.Controls.PluginExtension;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// MenuButton.xaml 的交互逻辑
    /// </summary>
    public partial class MenuButton : UserControl , INotifyPropertyChanged
    {
        public int IconSize
        {
            get { return (int)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize", typeof(int), typeof(MenuButton), new PropertyMetadata(12));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconBrushProperty =
            DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MenuButton), new PropertyMetadata(Brushes.White));

        public string BusyStatusDescription
        {
            get { return (string)GetValue(BusyStatusDescriptionProperty); }
            set { SetValue(BusyStatusDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BusyStatusDescription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BusyStatusDescriptionProperty =
            DependencyProperty.Register("BusyStatusDescription", typeof(string), typeof(MenuButton), new PropertyMetadata(""));

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public int TextSize
        {
            get { return (int)GetValue(TextSizeProperty); }
            set { SetValue(TextSizeProperty, value); }
        }

        public Thickness ContentMargin
        {
            get { return (Thickness)GetValue(ContentMarginProperty); }
            set { SetValue(ContentMarginProperty, value); }
        }

        private bool is_busy = false;

        public bool IsBusy
        {
            get => is_busy;
            set
            {
                is_busy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        // Using a DependencyProperty as the backing store for ContentMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentMarginProperty =
            DependencyProperty.Register("ContentMargin", typeof(Thickness), typeof(MenuButton), 
                new PropertyMetadata(new Thickness(40,2,10,2)));

        // Using a DependencyProperty as the backing store for TextSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSizeProperty =
            DependencyProperty.Register("TextSize", typeof(int), typeof(MenuButton), new PropertyMetadata(12));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(MenuButton), new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MenuButton), new PropertyMetadata(string.Empty));

        public event PropertyChangedEventHandler PropertyChanged;

        public MenuButton()
        {
            InitializeComponent();

            MainButton.DataContext = this;
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = IsBusy;
        }

        public ButtonBusyStatusDisposer BeginBusy() => new ButtonBusyStatusDisposer(this);

        public class ButtonBusyStatusDisposer:IDisposable
        {
            public MenuButton Button { get; }

            public void Dispose()
            {
                Button.IsBusy = false;
            }

            public ButtonBusyStatusDisposer(MenuButton button) 
            {
                Button = button;
                Button.IsBusy = true;
            }
        }
    }
}
