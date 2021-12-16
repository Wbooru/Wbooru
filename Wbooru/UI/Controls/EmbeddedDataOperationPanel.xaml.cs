using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
using Wbooru.Kernel.DI;
using Wbooru.Persistence;
using Wbooru.Settings;
using Wbooru.UI.Dialogs;
using Wbooru.Utils;

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

            if (!Setting<GlobalSetting>.Current.EnableFileCache)
                CacheFolderPanel.Visibility = Visibility.Collapsed;

            CalcCacheFolder();
        }

        private async void CalcCacheFolder()
        {
            CleanCacheFolderButton.IsEnabled = false;
            CacheFolderUsageText.Text = "正在计算中...";

            var len = await Task.Run(() => new DirectoryInfo(CacheFolderHelper.CacheFolderPath).EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories).Select(x =>
              {
                  try
                  {
                      var file = new FileInfo(x.FullName);
                      return file.Length;
                  }
                  catch
                  {
                      return 0;
                  }
              }).Sum());

            CleanCacheFolderButton.IsEnabled = true;
            CacheFolderUsageText.Text = FormatFileSize(len).ToString();
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "0B";
            var units = new[] { "B", "kB", "MB", "GB", "TB" };
            int digitGroups = (int)(Math.Log10(bytes) / Math.Log10(1024));
            return $"{(int)(bytes / Math.Pow(1024, digitGroups))}  {units[digitGroups]}";
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

                using var currentDBContext = new LocalDBContext();

                using var transaction = await currentDBContext.Database.BeginTransactionAsync();

                await Container.Get<IDownloadManager>().OnExit();

                CleanTable(currentDBContext.Downloads, "正在清理下载记录");
                CleanTable(currentDBContext.Tags, "正在清理标签数据");
                CleanTable(currentDBContext.ItemMarks, "正在清理收藏记录");
                CleanTable(currentDBContext.VisitRecords, "正在清理浏览记录");
                CleanTable(currentDBContext.GalleryItems, "正在清理图片数据缓存记录");

                await currentDBContext.SaveChangesAsync();
                await transaction.CommitAsync();

                task.Description = "清理完成，三秒后重启软件...";

                await Task.Delay(3000);

                App.UnusualSafeRestart();


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
            var command = $"-database_restore -from=\"{restore_file}\" -to=\"{System.IO.Path.GetFullPath(Setting<GlobalSetting>.Current.DBFilePath)}\"";

            Process.Start(Process.GetCurrentProcess().MainModule.FileName,command);
            App.UnusualSafeExit();
        }

        private async void CleanCacheFolder(object sender, RoutedEventArgs e)
        {
            CleanCacheFolderButton.IsEnabled = false;

            await Task.Run(() => 
            {
                foreach (var item in Directory.EnumerateFileSystemEntries(CacheFolderHelper.CacheFolderPath))
                {
                    if (File.Exists(item))
                    {
                        File.Delete(item);
                    }
                    else
                    {
                        Directory.Delete(item,true);
                    }
                }
            });

            CleanCacheFolderButton.IsEnabled = true;
            CalcCacheFolder();
            Toast.ShowMessage("清理完成");
        }

        private void Button_Click(object sender, RoutedEventArgs _)
        {
            OpenWithPath(CacheFolderHelper.CacheFolderPath);
        }

        private void Button_Click_1(object sender, RoutedEventArgs _)
        {
            OpenWithPath(System.IO.Path.GetFullPath(Setting<GlobalSetting>.Current.DownloadPath));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenWithPath(System.IO.Path.GetFullPath(Setting<GlobalSetting>.Current.LogOutputDirectory));
        }

        private static void OpenWithPath(string path)
        {
            try
            {
                Process.Start("explorer.exe" , System.IO.Path.GetFullPath(path));
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                Toast.ShowMessage("打开失败");
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            OpenWithPath(System.IO.Path.GetFullPath("Plugins"));
        }

        private async void PredownloadTagMetaButtonClick(object sender, RoutedEventArgs e)
        {
            var result = await Dialog.ShowComfirmDialog((sender as System.Windows.Controls.Button).ToolTip?.ToString(), "是否下载标签元数据?");

            if (!result)
                return;

            await Dialog.ShowDialog<TagMetaPredownloadProgressDisplayer>();
        }

        private async void CombineDatabase(object sender, RoutedEventArgs e)
        {
            await Dialog.ShowDialog<CombineDatabaseOption>();
        }
    }
}
