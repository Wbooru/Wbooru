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
using Wbooru.Utils;

namespace Wbooru.UI.Controls
{
    public partial class LoadingStatusDisplayer : UserControl
    {
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

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public string TaskCount
        {
            get { return (string)GetValue(TaskCountProperty); }
            set { SetValue(TaskCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskCountProperty =
            DependencyProperty.Register("TaskCount", typeof(string), typeof(LoadingStatusDisplayer), new PropertyMetadata(""));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(LoadingStatusDisplayer), new PropertyMetadata(""));

        public Brush ContentForeground
        {
            get { return (Brush)GetValue(ContentForegroundProperty); }
            set { SetValue(ContentForegroundProperty, value); }
        }

        public static readonly DependencyProperty ContentForegroundProperty =
            DependencyProperty.Register("ContentForeground", typeof(Brush), typeof(LoadingStatusDisplayer), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public LoadingTaskNotify BeginBusy(string description = null)
        {
            var t = ObjectPool<LoadingTaskNotify>.Get();

            t.Description = description ?? string.Empty;
            t.HostDisplayer = this;

            reg_notifies.Add(t);

            Dispatcher.InvokeAsync(() =>
            {
                Description = reg_notifies.Last().Description;

                show_action.Begin();
                UpdateCountString();
            });

            return t;
        }

        private void UpdateCountString()
        {
            TaskCount = reg_notifies.Count <= 1 ? "" : (reg_notifies.Count - 1).ToString();
        }

        internal void TaskFinishNotify(LoadingTaskNotify t)
        {
            reg_notifies.Remove(t);
            ObjectPool<LoadingTaskNotify>.Return(t);

            Dispatcher.InvokeAsync(() =>
            {
                if (reg_notifies.Count == 0)
                {
                    hide_action.Begin();
                }

                Description = reg_notifies.LastOrDefault()?.Description ?? Description;
                UpdateCountString();
            });
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
    }

    public class LoadingTaskNotify : IDisposable/*实现这个是为了方便using形式使用此控件*/
    {
        public LoadingStatusDisplayer HostDisplayer { get; set; }
        public string Description { get; set; }

        public void Dispose()
        {
            HostDisplayer.TaskFinishNotify(this);
            Log<LoadingTaskNotify>.Info($"Finish loading task:{Description}");
        }
    }
}
