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
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Kernel;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// DefaultLoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class DefaultLoginPage : CustomLoginPage
    {
        public override AccountInfo AccountInfo { get; protected set; }
        public Gallery Gallery { get; }

        public bool IsLoginRequesting
        {
            get { return (bool)GetValue(IsLoginRequestingProperty); }
            set { SetValue(IsLoginRequestingProperty, value); }
        }

        public static readonly DependencyProperty IsLoginRequestingProperty =
            DependencyProperty.Register("IsLoginRequesting", typeof(bool), typeof(DefaultLoginPage), new PropertyMetadata(false));

        public DefaultLoginPage(Gallery gallery)
        {
            InitializeComponent();
            Gallery = gallery;

            MainContent.DataContext = this;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AccountInfo.Password = PasswordInput.Password;

            Gallery?.Feature<IGalleryAccount>()?.AccountLogin(AccountInfo);
        }
    }
}
