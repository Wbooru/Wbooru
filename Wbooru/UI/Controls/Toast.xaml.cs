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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// Toast.xaml 的交互逻辑
    /// </summary>
    public partial class Toast : UserControl
    {
        private static Toast instance;

        public static void ShowMessage(string message, MessageType message_type = MessageType.Notify, uint show_time = 2000) => instance?.InternalShowMessage(message, message_type, show_time);

        public enum MessageType
        {
            Error,
            Warn,
            Notify
        }

        public SolidColorBrush TextColor
        {
            get { return (SolidColorBrush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(SolidColorBrush), typeof(Toast), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(Toast), new PropertyMetadata(""));

        private Storyboard sb;
        private DoubleAnimation animation;

        private Dictionary<MessageType, SolidColorBrush> Backgrounds = new Dictionary<MessageType, SolidColorBrush>()
        {
            {MessageType.Error,new SolidColorBrush(Colors.Red) },
            {MessageType.Warn,new SolidColorBrush(Colors.LightYellow) },
            {MessageType.Notify,new SolidColorBrush(Colors.White) },
        };

        public Toast()
        {
            if (instance != null)
                throw new Exception("not allow to create more Toast control.");

            InitializeComponent();

            ContentPanel.DataContext = this;

            sb = Resources["ShowAction"] as Storyboard;
            animation = sb.Children.FirstOrDefault(x => x.Name == "HideAnimation") as DoubleAnimation;

            instance = this;
        }

        private void InternalShowMessage(string message, MessageType message_type = MessageType.Notify, uint show_time = 2000)
        {
            Dispatcher.Invoke(() =>
            {
                Message = message;
                animation.BeginTime = TimeSpan.FromMilliseconds(show_time);
                TextColor = Backgrounds[message_type];
                Log<Toast>.Debug($"{message_type} {Message} ({animation.BeginTime})");
                sb.Begin();
            });
        }
    }
}
