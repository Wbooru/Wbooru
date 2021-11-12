using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;
using Wbooru.Models;

namespace Wbooru.Kernel
{
    public interface IDownloadManager : IManagerLifetime, IImplementInjectable
    {
        Task DownloadStart(DownloadWrapper download);
        Task DownloadDelete(DownloadWrapper download);
        Task DownloadPause(DownloadWrapper download);
        Task DownloadRestart(DownloadWrapper download);
        Task<bool> CheckIfContained(DownloadWrapper download); 

        ObservableCollection<DownloadWrapper> DownloadList { get; }
    }
}
