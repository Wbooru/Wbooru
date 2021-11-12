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

namespace Wbooru.Network
{
    [Export(typeof(ISchedulable))]
    [Export(typeof(ImageFetchDownloadScheduler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageFetchDownloadScheduler : ISchedulable, IImplementInjectable
    {
        public string SchedulerName => "Images Fetching Scheduler";

        public TimeSpan ScheduleCallLoopInterval { get; } = TimeSpan.FromSeconds(0.5);

        private List<Task<Image>> tasks_waiting_queue = new List<Task<Image>>();
        private HashSet<Task<Image>> tasks_running_queue = new HashSet<Task<Image>>();

        public Task<Image> DownloadImageAsync(string download_path, CancellationToken cancel_token = default, Action<(long downloaded_bytes, long content_bytes)> reporter = null, bool front_insert = false)
        {
            Task<Image> task = new Task<Image>(OnDownloadTaskStart, (download_path, reporter, cancel_token), cancel_token);

            lock (tasks_waiting_queue)
            {
                if (front_insert)
                    tasks_waiting_queue.Insert(0, task);
                else
                    tasks_waiting_queue.Insert(tasks_waiting_queue.Count, task);
            }

            return task;
        }

        public Task OnScheduleCall(CancellationToken cancellationToken)
        {
            var finished_tasks = tasks_running_queue.Where(t => t.Status != TaskStatus.Running).ToArray();

            foreach (var finished_task in finished_tasks)
                tasks_running_queue.Remove(finished_task);

            foreach (var except_task in finished_tasks.Where(x => x.IsFaulted))
            {
                lock (tasks_waiting_queue)
                {
                    tasks_waiting_queue.Insert(tasks_waiting_queue.Count, except_task);
                }
            }

            var add_count = Setting<GlobalSetting>.Current.LoadingImageThread - tasks_running_queue.Count;

            for (int i = 0; (i < add_count) && tasks_waiting_queue.Count > 0; i++)
            {
                Task<Image> task;

                lock (tasks_waiting_queue)
                {
                    task = tasks_waiting_queue.First();
                    tasks_waiting_queue.Remove(task);
                }

                task.Start();

                tasks_running_queue.Add(task);
            }

            return Task.CompletedTask;
        }

        private Image OnDownloadTaskStart(object state)
        {
            try
            {
                (string download_path, Action<(long downloaded_bytes, long content_bytes)> reporter, CancellationToken cancelToken) = (ValueTuple<string, Action<(long, long)>, CancellationToken>)state;
                if (cancelToken.IsCancellationRequested)
                    return default;

                Log<ImageFetchDownloadScheduler>.Info($"Start download image:{download_path}");

                var response = RequestHelper.CreateDeafultAsync(download_path).ConfigureAwait(false).GetAwaiter().GetResult();

                var content_length = response.ContentLength;

                using var stream = response.GetResponseStream().Interopable();

                int total_read = 0;

                stream.OnAfterRead += (buffer, offset, count, read) =>
                {
                    total_read += read;
                    reporter?.Invoke((total_read, content_length));
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
