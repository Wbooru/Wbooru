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

        private bool is_downloading;

        public bool IsDownloading
        {
            get => is_downloading;
            set
            {
                is_downloading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDownloading)));
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
