using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Wbooru.UI.Dialogs
{
    /// <summary>
    /// TagMetaPredownloadProgressDisplayer.xaml 的交互逻辑
    /// </summary>
    public partial class TagMetaPredownloadProgressDisplayer : DialogContentBase,INotifyPropertyChanged
    {
        public CacheTagMetaProgressStatus Status
        {
            get { return (CacheTagMetaProgressStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public Timer Timer { get; }

        private int speed;

        public int Speed
        {
            get => speed;
            set
            {
                speed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Speed)));
            }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(CacheTagMetaProgressStatus), typeof(TagMetaPredownloadProgressDisplayer), new PropertyMetadata(default));

        public TagMetaPredownloadProgressDisplayer()
        {
            InitializeComponent();

            Status = TagManager.StartCacheTagMeta();

            Timer = new Timer(CalculateSpeed, null, 0, 5000);

            AwaitTaskEnd();
        }

        private async void AwaitTaskEnd()
        {
            await Status.Task;

            if (!Status.RequestCancel)
                await Dialog.ShowDialog("已完成所有标签元数据预缓存");

            Dialog.CloseDialog(this);
        }

        private void TagMetaPredownloadProgressDisplayer_Unloaded(object sender, RoutedEventArgs e)
        {
            Timer.Dispose();
        }

        int prev_added = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void CalculateSpeed(object _)
        {
            var now = Dispatcher.Invoke(() => Status.AddedCount);
            var diff = now - prev_added;

            Speed = diff * (60 / 5);

            prev_added = now;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Info("User cancelled operation");
            Status.CancelTask();
        }
    }
}
