using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.Kernel
{
    public class CacheTagMetaProgressStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int search_count;

        public int SearchCount
        {
            get => search_count;
            set
            {
                search_count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchCount)));
            }
        }

        private string current_searching_name;

        public string CurrentSearchingName
        {
            get => current_searching_name;
            set
            {
                current_searching_name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSearchingName)));
            }
        }

        private int added_count;

        public int AddedCount
        {
            get => added_count;
            set
            {
                added_count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddedCount)));
            }
        }

        public Task Task { get; set; }

        private int finished_count;

        public int FinishedCount
        {
            get => finished_count;
            set
            {
                finished_count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinishedCount)));
            }
        }

        public bool RequestCancel { get; private set; } = false;

        public void CancelTask()
        {
            RequestCancel = true;
        }
    }
}