using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Wbooru.Utils;

namespace Wbooru.UI.Controls
{
    public partial class LoadingStatusDisplayer : UserControl, INotifyPropertyChanged
    {
        private bool has_task_running;

        public bool HasTaskRunning
        {
            get => has_task_running;
            private set
            {
                has_task_running = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTaskRunning)));
                OnHasTaskRunningChanged();
            }
        }

        public LoadingStatusDisplayer()
        {
            InitializeComponent();
            trans = Resources["Trans"] as Storyboard;

            Loaded += (a, b) => Active();

            MainPanel.DataContext = this;
            show_action = Resources["ShowAction"] as Storyboard;
            hide_action = Resources["HideAction"] as Storyboard;
        }

        HashSet<LoadingTaskNotify> reg_notifies = new HashSet<LoadingTaskNotify>();
        private Storyboard show_action;
        private Storyboard hide_action;
        private readonly Storyboard trans;

        public LoadingTaskNotify CurrentTaskNotify
        {
            get { return (LoadingTaskNotify)GetValue(CurrentTaskNotifyProperty); }
            set { SetValue(CurrentTaskNotifyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTaskNotify.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTaskNotifyProperty =
            DependencyProperty.Register("CurrentTaskNotify", typeof(LoadingTaskNotify), typeof(LoadingStatusDisplayer), new PropertyMetadata(null,(d,e)=> (d as LoadingStatusDisplayer).HasTaskRunning = e.NewValue != null));

        public string TaskCount
        {
            get { return (string)GetValue(TaskCountProperty); }
            set { SetValue(TaskCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskCountProperty =
            DependencyProperty.Register("TaskCount", typeof(string), typeof(LoadingStatusDisplayer), new PropertyMetadata(""));

        public Brush ContentForeground
        {
            get { return (Brush)GetValue(ContentForegroundProperty); }
            set { SetValue(ContentForegroundProperty, value); }
        }

        public static readonly DependencyProperty ContentForegroundProperty =
            DependencyProperty.Register("ContentForeground", typeof(Brush), typeof(LoadingStatusDisplayer), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public event PropertyChangedEventHandler PropertyChanged;

        public LoadingTaskNotify BeginBusy(string description = null)
        {
            var t = new LoadingTaskNotify();

            t.Description = description ?? string.Empty;
            t.HostDisplayer = this;

            reg_notifies.Add(t);

            UpdateTopTaskNotify();

            return t;
        }

        private void OnHasTaskRunningChanged()
        {
            if (HasTaskRunning)
                show_action.Begin();
            else
                hide_action.Begin();

            UpdateCountString();
        }

        private void UpdateCountString()
        {
            TaskCount = reg_notifies.Count <= 1 ? "" : (reg_notifies.Count - 1).ToString();
        }

        private void UpdateTopTaskNotify()
        {
            Dispatcher.InvokeAsync(() => CurrentTaskNotify = reg_notifies.LastOrDefault());
        }

        internal void TaskFinishNotify(LoadingTaskNotify t)
        {
            //avoid clean.
            if (!reg_notifies.Remove(t))
            {
                Log<LoadingStatusDisplayer>.Debug($"skip cleaning this notify object[{t.NotifyID}].because it has already been cleaned.");
                return;
            }

            Log<LoadingStatusDisplayer>.Debug($"removed all clean notify object[{t.NotifyID}]");

            UpdateTopTaskNotify();
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

        public bool IsBusy => reg_notifies.Count!=0;

        public void ForceFinishAllStatus()
        {
            foreach (var task in reg_notifies.ToArray())
            {
                task.Dispose();
            }
        }
    }

    public sealed class LoadingTaskNotify : IDisposable/*实现这个是为了方便using形式使用此控件*/,INotifyPropertyChanged
    {
        private static uint ID=0;

        public LoadingTaskNotify()
        {
            ID = (++ID) % uint.MaxValue;
            NotifyID = ID;
            StartTime = DateTime.Now;
        }

        public LoadingStatusDisplayer HostDisplayer { get; set; }

        private string _description;
        public string Description
        {
            get => _description; set
            {
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public uint NotifyID { get; private set; }
        public DateTime StartTime { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            HostDisplayer.TaskFinishNotify(this);
            Log<LoadingTaskNotify>.Info($"Finish loading task({(DateTime.Now - StartTime).TotalMilliseconds} ms):{Description}");
        }
    }
}
