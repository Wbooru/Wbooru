using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Wbooru.Kernel;
using Wbooru.Settings;
using Wbooru.UI.Dialogs;
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
            Log.Info("Begin init MainWindow");

            InitializeComponent();

            Dialog.Init(DialogLayer, WindowFrame);

            setting = SettingManager.LoadSetting<GlobalSetting>();

            if (setting.RememberWindowSizeAndLocation)
            {
                //restore size
                if (setting.WindowSize != null)
                {
                    Width = Math.Max(0, setting.WindowSize.Value.Width);
                    Height = Math.Max(0, setting.WindowSize.Value.Height);
                }

                //restore location
                if (setting.WindowLocation != null)
                {
                    Left = Math.Max(0,setting.WindowLocation.Value.X);
                    Top = Math.Max(0, setting.WindowLocation.Value.Y);
                }
            }

            NavigationHelper.InitNavigationHelper(WindowFrame);
            //NavigationHelper.NavigationPush(new _MainPage());
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

            Log.Info("Initialized MainWindow");
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
