using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Models.SettingUI;
using Wbooru.Settings.UIAttributes;

namespace Wbooru.UI.Controls.SettingUI
{
    /// <summary>
    /// PathSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PathSetting : UserControl
    {
        public PropertyInfoWrapper Wrapper { get; }

        public string ProxyValue
        {
            get { return (string)GetValue(ProxyValueProperty); }
            set { SetValue(ProxyValueProperty, value); }
        }

        public static readonly DependencyProperty ProxyValueProperty =
            DependencyProperty.Register("ProxyValue", typeof(string), typeof(PathSetting), new PropertyMetadata(default, (d, e) => {
                (d as PathSetting).Wrapper.ProxyValue = e.NewValue;
            }));

        PathAttribute path_attr;
        private OpenFileDialog dialog;

        public PathSetting(PropertyInfoWrapper wrapper)
        {
            InitializeComponent();
            Wrapper = wrapper;

            if (!(wrapper.PropertyInfo.GetCustomAttribute<PathAttribute>() is PathAttribute path))
                throw new Exception("PathSetting钦定设置属性必须有[Path]特性标识");

            path_attr = path;

            MainPanel.DataContext = this;

            SetValue(ProxyValueProperty, wrapper.ProxyValue);

            dialog = new OpenFileDialog();

            dialog.CheckFileExists = path_attr.IsFilePath;
            dialog.Filter = path_attr.ExtFilter;
            dialog.DefaultExt = path_attr.DefaultExt;

            NameBlock.Text = wrapper.DisplayPropertyName;

            if (wrapper.PropertyInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute description)
                NameBlock.ToolTip = description.Description;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dialog.ShowDialog(Window.GetWindow(this))??false)
            {
                var select_path = dialog.FileName;

                if (path_attr.MustExist && !(path_attr.IsFilePath?File.Exists(select_path):Directory.Exists(select_path)))
                {
                    Container.Default.GetExportedValue<Toast>().ShowMessage($"选择的{(path_attr.IsFilePath?"文件":"文件夹")}必须是存在的.");
                    return;
                }

                ProxyValue = select_path;
            }
        }
    }
}
