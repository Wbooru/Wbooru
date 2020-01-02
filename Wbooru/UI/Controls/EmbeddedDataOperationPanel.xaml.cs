using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
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
using Wbooru.Kernel;
using Wbooru.Persistence;
using Wbooru.Settings;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// EmbeddedDataOperationPanel.xaml 的交互逻辑
    /// </summary>
    public partial class EmbeddedDataOperationPanel : System.Windows.Controls.UserControl
    {
        public EmbeddedDataOperationPanel()
        {
            InitializeComponent();
        }

        private void BackupDatabase(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "MyBackupDataFile";
            dialog.Title = "请选择备份文件来恢复.";
            dialog.Filter = "Wbooru 数据备份文件(*.wdbk)|*.wdbk";

            if (!(dialog.ShowDialog() == DialogResult.OK && dialog.FileName is string backup_file && !string.IsNullOrWhiteSpace(backup_file)))
                return;

            var command = $"-database_backup -to=\"{backup_file}\"";

            Process.Start(Process.GetCurrentProcess().MainModule.FileName, command);
            App.UnusualSafeExit();
        }

        private async void EraseDatabase(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("确定要清空数据库?所有保存的数据都会被清除.", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (var task = StatusDisplayer.BeginBusy("三秒后开始清除..."))
            {
                await Task.Delay(3000);

                using (var transaction = LocalDBContext.Instance.Database.BeginTransaction())
                {
                    DownloadManager.Close();

                    CleanTable(LocalDBContext.Instance.Downloads, "正在清理下载记录");
                    CleanTable(LocalDBContext.Instance.Tags, "正在清理标签数据");
                    CleanTable(LocalDBContext.Instance.ItemMarks, "正在清理收藏记录");
                    CleanTable(LocalDBContext.Instance.VisitRecords, "正在清理浏览记录");
                    CleanTable(LocalDBContext.Instance.ShadowGalleryItems, "正在清理图片数据缓存记录");

                    await LocalDBContext.Instance.SaveChangesAsync();
                    transaction.Commit();

                    task.Description = "清理完成，三秒后重启软件...";

                    await Task.Delay(3000);

                    App.UnusualSafeRestart();
                }

                async void CleanTable<T>(DbSet<T> set, string description) where T : class
                {
                    task.Description = description;
                    set.RemoveRange(set);
                    await Task.Delay(1000);
                }
            }
        }

        private void RestoreDatabase(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择备份文件来恢复.";
            dialog.Filter = "Wbooru 数据备份文件(*.wdbk)|*.wdbk";

            if (!(dialog.ShowDialog() == DialogResult.OK && dialog.FileName is string restore_file && File.Exists(restore_file)))
                return;

            //build command and params
            var command = $"-database_restore -from=\"{restore_file}\" -to=\"{System.IO.Path.GetFullPath(SettingManager.LoadSetting<GlobalSetting>().DBFilePath)}\"";

            Process.Start(Process.GetCurrentProcess().MainModule.FileName,command);
            App.UnusualSafeExit();
        }
    }
}
