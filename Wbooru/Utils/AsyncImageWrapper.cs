using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Wbooru.Network;
using Wbooru.Utils.Resource;

namespace Wbooru.Utils
{
    public class AsyncImageWrapper : INotifyPropertyChanged
    {
#if DEBUG
        string name;
        string download_link;
#endif

        public AsyncImageWrapper(string name, string dl) : this(async () =>
          {
              return (await ImageResourceManager.RequestImageAsync(name, dl, false))?.ConvertToBitmapImage();
          })
        {
#if DEBUG
            this.name = name;
            download_link = dl;
#endif
            //momo moe~
        }

        public AsyncImageWrapper(Func<Task<BitmapImage>> valueFunc)
        {
            LoadValue(valueFunc);
        }

        private async void LoadValue(Func<Task<BitmapImage>> valueFunc)
        {
            AsyncValue = await Task.Run(() =>
            {
                return valueFunc();
            });
        }

        public BitmapImage asyncValue;
        public BitmapImage AsyncValue
        {
            get { return asyncValue; }
            set
            {
                asyncValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AsyncValue)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
