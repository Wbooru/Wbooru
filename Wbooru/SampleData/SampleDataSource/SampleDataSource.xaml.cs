//   *********  请勿修改此文件   *********
//   此文件由设计工具再生成。更改
//   此文件可能会导致错误。
namespace Expression.Blend.SampleData.SampleDataSource
{
    using System; 
    using System.ComponentModel;

// 若要在生产应用程序中显著减小示例数据涉及面，则可以设置
// DISABLE_SAMPLE_DATA 条件编译常量并在运行时禁用示例数据。
#if DISABLE_SAMPLE_DATA
    internal class SampleDataSource { }
#else

    public class SampleDataSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SampleDataSource()
        {
            try
            {
                Uri resourceUri = new Uri("ms-appx:/SampleData/SampleDataSource/SampleDataSource.xaml", UriKind.RelativeOrAbsolute);
                System.Windows.Application.LoadComponent(this, resourceUri);
            }
            catch
            {
            }
        }

        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }
    }

    public class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private System.Windows.Media.ImageSource _Property1 = null;

        public System.Windows.Media.ImageSource Property1
        {
            get
            {
                return this._Property1;
            }

            set
            {
                if (this._Property1 != value)
                {
                    this._Property1 = value;
                    this.OnPropertyChanged("Property1");
                }
            }
        }
    }

    public class ItemCollection : System.Collections.ObjectModel.ObservableCollection<Item>
    { 
    }
#endif
}
