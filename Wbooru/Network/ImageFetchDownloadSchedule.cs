using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Wbooru.PluginExt;
using Wbooru.Settings;

namespace Wbooru.Network
{
    [Export(typeof(ISchedulable))]
    [Export(typeof(ImageFetchDownloadSchedule))]
    public class ImageFetchDownloadSchedule : ISchedulable
    {
        [ImportingConstructor]
        public ImageFetchDownloadSchedule([Import]SettingManager setting_manager)
        {
            setting = setting_manager.LoadSetting<GlobalSetting>();
        }

        public bool IsAsyncSchedule => false;

        private Queue<Task<Image>> tasks_waiting_queue=new Queue<Task<Image>>();
        private HashSet<Task<Image>> tasks_running_queue=new HashSet<Task<Image>>();

        private GlobalSetting setting;

        public Task<Image> GetImageAsync(string download_path)
        {
            var task = new Task<Image>(OnDownloadTaskStart, download_path);

            tasks_waiting_queue.Enqueue(task);

            return task;
        }

        public void OnScheduleCall()
        {
            tasks_running_queue.RemoveWhere(t => t.Status != TaskStatus.Running);

            var add_count = setting.LoadingImageThread - tasks_running_queue.Count;

            for (int i = 0; (i < add_count) && tasks_waiting_queue.Count > 0 ; i++)
            {
                var task = tasks_waiting_queue.Dequeue();

                task.Start();

                tasks_running_queue.Add(task);
            }
        }

        private Image OnDownloadTaskStart(object state)
        {
            var download_path = (string)state;

            Log<ImageFetchDownloadSchedule>.Info($"Start download image:{download_path}");

            var response = RequestHelper.CreateDeafult(download_path);

            using var stream = response.GetResponseStream();

            Image source=Image.FromStream(stream);

            return source;
        }
    }
}
