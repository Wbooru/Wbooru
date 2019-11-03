using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Models;
using Wbooru.Network;
using Wbooru.Persistence;
using Wbooru.UI.Controls;
using Wbooru.Utils;

namespace Wbooru.Kernel
{
    public static class DownloadManager
    {
        public static ObservableCollection<DownloadWrapper> DownloadList { get; } = new ObservableCollection<DownloadWrapper>();
        
        internal static void Close()
        {
            var db = Container.Default.GetExportedValue<LocalDBContext>();

            foreach (var item in DownloadList)
            {
                if (db.Downloads.Find(item.DownloadInfo.DownloadId) is Download entity)
                {
                    Log.Debug($"Update download record :{item.DownloadInfo.DownloadFullPath}");
                    db.Entry(entity).CurrentValues.SetValues(item.DownloadInfo);
                }
                else
                {
                    Log.Debug($"Add download record :{item.DownloadInfo.DownloadFullPath}");
                    db.Downloads.Add(item.DownloadInfo);
                }

            }

            db.SaveChanges();
        }

        internal static void LoadSavedDownloadList()
        {
            var db = Container.Default.GetExportedValue<LocalDBContext>();

            foreach (var item in db.Downloads.Select(x => new DownloadWrapper()
            {
                DownloadInfo = x,
                CurrentDownloadedLength = x.DisplayDownloadedLength
            }))
            {
                Log.Debug($"Load download record :{item.DownloadInfo.DownloadFullPath}");
                DownloadList.Add(item);
            }
        }

        public static void DownloadStart(DownloadWrapper download)
        {
            if (download.IsDownloading)
            {
                Log.Info($"Download task {download.DownloadInfo.GalleryName} has already been started.");
                return;
            }

            //check
            if (string.IsNullOrWhiteSpace(download.DownloadInfo.DownloadFullPath))
            {
                var message = "此任务出现格式错误，没钦定下载路径";
                ExceptionHelper.DebugThrow(new Exception(message));
                Container.Default.GetExportedValue<Toast>().ShowMessage(message);

                download.ErrorMessage = message;
                DownloadPause(download);

                return;
            }

            download.ErrorMessage = string.Empty;

            var cancel_token_source = new CancellationTokenSource();
            download.CancelTokenSource = cancel_token_source;

            var task = Task.Run(()=>OnDownloadTaskStart(download), cancel_token_source.Token);

            download.IsDownloading = true;
        }

        private static void OnDownloadTaskStart(DownloadWrapper download)
        {
            try
            {
                using var file_stream = File.OpenWrite(download.DownloadInfo.DownloadFullPath);
                download.CurrentDownloadedLength = file_stream.Length;
                file_stream.Seek(download.CurrentDownloadedLength, SeekOrigin.Begin);

                var response = RequestHelper.CreateDeafult(download.DownloadInfo.DownloadUrl, request =>
                {
                    if (download.CurrentDownloadedLength > 0)
                        request.AddRange(download.CurrentDownloadedLength);
                });

                using var response_stream = response.GetResponseStream();

                var buffer = new byte[1024];
                int read_bytes = 0;

                do
                {
                    read_bytes = response_stream.Read(buffer, 0, buffer.Length);
                    file_stream.Write(buffer, 0, read_bytes);
                    download.CurrentDownloadedLength += read_bytes;
                } while (read_bytes != 0);

                //downloading finished
                download.IsDownloading = false;
            }
            catch (Exception e)
            {
                //error ocured
                download.ErrorMessage = e.Message;
                DownloadPause(download);
            }
        }

        public static void DownloadPause(DownloadWrapper download)
        {
            if (!download.IsDownloading)
            {
                Log.Info($"Download task {download.DownloadInfo.GalleryName} has already been paused.");
                return;
            }

            download.IsDownloading = false;
            download.CancelTokenSource?.Cancel();
        }
    }
}
