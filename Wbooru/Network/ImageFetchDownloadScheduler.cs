using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Wbooru.PluginExt;
using Wbooru.Settings;
using Wbooru.UI.Controls;
using System.Threading;
using System.IO;
using Wbooru.Utils;
using Wbooru.Kernel.DI;
using System.Net;

namespace Wbooru.Network
{
    [Export(typeof(ISchedulable))]
    [Export(typeof(ImageFetchDownloadScheduler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageFetchDownloadScheduler : ISchedulable, IImplementInjectable
    {
        public struct ImageDownloadTaskInfo
        {
            private Action<(long downloaded_bytes, long content_bytes)> reporter;

            public string Url { get; set; }
            public Task<Image> Task { get; set; }
            public long DownloadedBytes { get; set; }
            public long TotalContentBytes { get; set; }

            public ImageDownloadTaskInfo(string url, Action<(long downloaded_bytes, long content_bytes)> reporter, Task<Image> task)
            {
                this.reporter = reporter;

                Url = url;
                Task = task;
                DownloadedBytes = 0;
                TotalContentBytes = 0;
            }

            public void ReportDownloadingProgress(long? downloadedBytes = default, long? totalContentBytes = default)
            {
                DownloadedBytes = downloadedBytes ?? DownloadedBytes;
                TotalContentBytes = totalContentBytes ?? TotalContentBytes;

                reporter?.Invoke((DownloadedBytes, TotalContentBytes));
            }
        }

        public string SchedulerName => "Images Fetching Scheduler";

        public TimeSpan ScheduleCallLoopInterval { get; } = TimeSpan.FromSeconds(0.5);

        private List<ImageDownloadTaskInfo> tasks_waiting_queue = new();
        private HashSet<ImageDownloadTaskInfo> tasks_running_queue = new();

        public IEnumerable<ImageDownloadTaskInfo> TasksWaitingQueue => tasks_waiting_queue;
        public IEnumerable<ImageDownloadTaskInfo> TasksRunningQueue => tasks_running_queue;

        public Task<Image> DownloadImageAsync(string download_path, CancellationToken cancel_token = default, Action<(long downloaded_bytes, long content_bytes)> reporter = null, Action<HttpWebRequest> customReqFunc = default, bool front_insert = false)
        {
            var taskInfo = new ImageDownloadTaskInfo(download_path, reporter, null);
            Task<Image> task = new Task<Image>(OnDownloadTaskStart, (taskInfo, cancel_token, customReqFunc), cancel_token);
            taskInfo.Task = task;

            lock (tasks_waiting_queue)
            {
                if (front_insert)
                    tasks_waiting_queue.Insert(0, taskInfo);
                else
                    tasks_waiting_queue.Insert(tasks_waiting_queue.Count, taskInfo);
            }

            return task;
        }

        public Task OnScheduleCall(CancellationToken cancellationToken)
        {
            var finished_tasks = tasks_running_queue.Where(t => t.Task.Status != TaskStatus.Running).ToArray();

            foreach (var finished_task in finished_tasks)
                tasks_running_queue.Remove(finished_task);

            foreach (var except_task in finished_tasks.Where(x => x.Task.IsFaulted))
            {
                lock (tasks_waiting_queue)
                {
                    tasks_waiting_queue.Insert(tasks_waiting_queue.Count, except_task);
                }
            }

            var add_count = Setting<GlobalSetting>.Current.LoadingImageThread - tasks_running_queue.Count;

            for (int i = 0; (i < add_count) && tasks_waiting_queue.Count > 0; i++)
            {
                ImageDownloadTaskInfo task;

                lock (tasks_waiting_queue)
                {
                    task = tasks_waiting_queue.First();
                    tasks_waiting_queue.Remove(task);
                }

                task.Task.Start();

                tasks_running_queue.Add(task);
            }

            return Task.CompletedTask;
        }

        private Image OnDownloadTaskStart(object state)
        {
            try
            {
                (ImageDownloadTaskInfo taskInfo, CancellationToken cancelToken, Action<HttpWebRequest> customReqFunc) = (ValueTuple<ImageDownloadTaskInfo, CancellationToken, Action<HttpWebRequest>>)state;
                if (cancelToken.IsCancellationRequested)
                    return default;

                Log<ImageFetchDownloadScheduler>.Info($"Start download image:{taskInfo.Url}");

                var response = RequestHelper.CreateDeafultAsync(taskInfo.Url, customReqFunc).ConfigureAwait(false).GetAwaiter().GetResult();

                var content_length = response.ContentLength;

                using var stream = response.GetResponseStream().Interopable();

                int total_read = 0;

                stream.OnAfterRead += (buffer, offset, count, read) =>
                {
                    total_read += read;
                    taskInfo.ReportDownloadingProgress(total_read, content_length);
                };

                Image source = Image.FromStream(stream);

                return source;
            }
            catch (Exception e)
            {
                Log<ImageFetchDownloadScheduler>.Error($"Can't download image ({e.Message}):{state}");
                //Toast.ShowMessage($"无法下载图片({e.Message})");
                return default;
            }
        }

        public void OnSchedulerTerm()
        {

        }
    }
}
