using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Models.SettingUI;
using Wbooru.Settings.UIAttributes;
using Wbooru.UI.ValueConverters.SettingUI;
using Wbooru.Settings;

namespace Wbooru.UI.Controls.SettingUI
{
    /// <summary>
    /// CommonSetting.xaml 的交互逻辑
    /// </summary>
    public partial class CommonValueSetting : UserControl
    {
        public PropertyInfoWrapper Wrapper
        {
            get { return (PropertyInfoWrapper)GetValue(WrapperProperty); }
            set { SetValue(WrapperProperty, value); }
        }

        public static readonly DependencyProperty WrapperProperty =
            DependencyProperty.Register("Wrapper", typeof(PropertyInfoWrapper), typeof(CommonValueSetting), new PropertyMetadata(null));

        public CommonValueSetting(PropertyInfoWrapper wrapper)
        {
            InitializeComponent();

            MainPanel.DataContext = this;
            Wrapper = wrapper;

            Binding binding = new Binding();
            binding.Source = Wrapper;
            binding.Path = new PropertyPath("ProxyValue");
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding.Converter = new AutoValueConverter();
            binding.ConverterParameter = Wrapper;

            Input.Text = Wrapper.ProxyValue.ToString();
            Input.SetBinding(TextBox.TextProperty, binding);

            if (wrapper.PropertyInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute description)
                NameBlock.ToolTip = description.Description;

            NameBlock.Text = wrapper.DisplayPropertyName;
        }
    }
}
