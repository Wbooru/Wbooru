using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Models.Gallery;
using Wbooru.Persistence;

namespace Wbooru.UI.Dialogs
{
    /// <summary>
    /// SelectableImageList.xaml 的交互逻辑
    /// </summary>
    public partial class CombineDatabaseOption : DialogContentBase
    {
        public CombineDatabaseOption()
        {
            InitializeComponent();
        }

        public string DBFilePath
        {
            get { return (string)GetValue(DBFilePathProperty); }
            set { SetValue(DBFilePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DBFilePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DBFilePathProperty =
            DependencyProperty.Register("DBFilePath", typeof(string), typeof(CombineDatabaseOption), new PropertyMetadata(""));

        public bool[] CombineTargetFlags
        {
            get { return (bool[])GetValue(CombineTargetFlagsProperty); }
            set { SetValue(CombineTargetFlagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CombineTargetFlags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CombineTargetFlagsProperty =
            DependencyProperty.Register("CombineTargetFlags", typeof(bool[]), typeof(CombineDatabaseOption), new PropertyMetadata(new bool[] { true, true, true, true, true }));

        public bool IsExecuting
        {
            get { return (bool)GetValue(IsExecutingProperty); }
            set { SetValue(IsExecutingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExecuting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExecutingProperty =
            DependencyProperty.Register("IsExecuting", typeof(bool), typeof(CombineDatabaseOption), new PropertyMetadata(false));

        public string ProgressReportMessage
        {
            get { return (string)GetValue(ProgressReportMessageProperty); }
            set { SetValue(ProgressReportMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressReportMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressReportMessageProperty =
            DependencyProperty.Register("ProgressReportMessage", typeof(string), typeof(CombineDatabaseOption), new PropertyMetadata(""));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.CheckFileExists = true;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            DBFilePath = dialog.FileName;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ProgressReportMessage = "";
            IsExecuting = true;
            //get flags
            var flag = 0;
            for (int i = 0; i < CombineTargetFlags.Length; i++)
                flag = flag | ((CombineTargetFlags[i] ? 1 : 0) << i);
            var combineFlags = (DatabaseCombinePart)flag;

            await LocalDBContext.CombineDatabase(
                DBFilePath,
                (msg) => Dispatcher.Invoke(() => ProgressReportMessage = ProgressReportMessage + Environment.NewLine + msg),
                combineFlags
            );

            IsExecuting = false;

            if (await Dialog.ShowDialog("合并完成", "合并数据库", "关闭窗口", "返回"))
                Dialog.CloseDialog(this);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Dialog.CloseDialog(this);
        }
    }
}
