using System;
using System.Windows;
using System.Windows.Input;
using Wbooru.Kernel;
using Wbooru.Settings;
using Wbooru.UI.Pages;

namespace Wbooru.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        GlobalSetting setting;

        public MainWindow()
        {
            InitializeComponent();

            setting = SettingManager.LoadSetting<GlobalSetting>();

            if (setting.RememberWindowSizeAndLocation)
            {
                //restore size
                if (setting.WindowSize != null)
                {
                    Width = setting.WindowSize.Value.Width;
                    Height = setting.WindowSize.Value.Height;
                }

                //restore location
                if (setting.WindowLocation != null)
                {
                    Left = setting.WindowLocation.Value.X;
                    Top = setting.WindowLocation.Value.Y;
                }
            }

            NavigationHelper.InitNavigationHelper(WindowFrame);
            NavigationHelper.NavigationPush(new MainGalleryPage());

            //disable navigation actions
            foreach (var vNavigationCommand in new RoutedUICommand[]
                {   
                    //NavigationCommands.BrowseBack,
                    //NavigationCommands.BrowseForward,
                    NavigationCommands.BrowseHome,
                    NavigationCommands.BrowseStop,
                    NavigationCommands.Refresh,
                    NavigationCommands.Favorites,
                    NavigationCommands.Search,
                    NavigationCommands.IncreaseZoom,
                    NavigationCommands.DecreaseZoom,
                    NavigationCommands.Zoom,
                    NavigationCommands.NextPage,
                    NavigationCommands.PreviousPage,
                    NavigationCommands.FirstPage,
                    NavigationCommands.LastPage,
                    NavigationCommands.GoToPage,
                    NavigationCommands.NavigateJournal })
            {
                WindowFrame.CommandBindings.Add(new CommandBinding(vNavigationCommand, (sender, args) => { }));
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            App.Term();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setting.WindowSize = e.NewSize;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            setting.WindowLocation = new Point(Left, Top);
        }

        private void OnRequestNavigateBack(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            NavigationHelper.RequestPageBackAction();
        }

        private void OnRequestNavigateForward(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            NavigationHelper.RequestPageForwardAction();
        }
    }
}
