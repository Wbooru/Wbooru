using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.Models
{
    public class DownloadWrapper: INotifyPropertyChanged
    {
        private Download download_info;

        public Download DownloadInfo
        {
            get => download_info;
            set
            {
                download_info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadInfo)));
            }
        }

        private DownloadTaskStatus status;

        public DownloadTaskStatus Status
        {
            get => status;
            set
            {
                status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public CancellationTokenSource CancelTokenSource { get; set; }

        private string error_message;

        public string ErrorMessage
        {
            get => error_message;
            set
            {
                error_message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }

        private long current_downloaded_length;

        public long CurrentDownloadedLength
        {
            get => current_downloaded_length;
            set
            {
                current_downloaded_length = DownloadInfo.DisplayDownloadedLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDownloadedLength)));
            }
        }

        private long download_speed;

        /// <summary>
        /// 勿碰.jpg
        /// </summary>
        public long DownloadSpeed
        {
            get => download_speed;
            set
            {
                download_speed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadSpeed)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSaveInDB => DownloadInfo.DownloadId > 0;
    }
}
