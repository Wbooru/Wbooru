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
using Wbooru.Utils;

namespace Wbooru.UI.Controls
{
    public partial class LoadingStatusDisplayer : UserControl
    {
        public LoadingStatusDisplayer()
        {
            InitializeComponent();

            DataContext = this;
        }

        HashSet<LoadingTaskNotify> reg_notifies = new HashSet<LoadingTaskNotify>();

        public LoadingTaskNotify BeginBusy(string description = null)
        {
            var t = ObjectPool<LoadingTaskNotify>.Get();

            t.Description = description ?? string.Empty;
            t.HostDisplayer = this;

            reg_notifies.Add(t);

            return t;
        }

        internal void TaskFinishNotify(LoadingTaskNotify t)
        {
            reg_notifies.Remove(t);
            ObjectPool<LoadingTaskNotify>.Return(t);
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
