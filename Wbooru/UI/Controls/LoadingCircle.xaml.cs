using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// LoadingCircle.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingCircle : UserControl
    {
        private readonly Storyboard trans;

        public LoadingCircle()
        {
            InitializeComponent();
            trans = Resources["Trans"] as Storyboard;
            Loaded += (a, b) => Active();
        }

        private async void Active()
        {
            el.BeginStoryboard(trans);
            await Task.Delay(170);
            el2.BeginStoryboard(trans);
            await Task.Delay(170);
            el3.BeginStoryboard(trans);
            await Task.Delay(170);
            el4.BeginStoryboard(trans);
            await Task.Delay(170);
            el5.BeginStoryboard(trans);
            await Task.Delay(170);
            el6.BeginStoryboard(trans);
        }
    }
}
