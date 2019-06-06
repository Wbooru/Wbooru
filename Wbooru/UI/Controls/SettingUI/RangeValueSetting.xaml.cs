using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;
using Wbooru.UI.ValueConverters.SettingUI;

namespace Wbooru.UI.Controls.SettingUI
{
    /// <summary>
    /// RangeValueSetting.xaml 的交互逻辑
    /// </summary>
    public partial class RangeValueSetting : UserControl , INotifyPropertyChanged
    {
        public PropertyInfoWrapper Wrapper
        {
            get { return (PropertyInfoWrapper)GetValue(WrapperProperty); }
            set { SetValue(WrapperProperty, value); }
        }

        public static readonly DependencyProperty WrapperProperty =
            DependencyProperty.Register("Wrapper", typeof(PropertyInfoWrapper), typeof(RangeValueSetting), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public object RefValue { get => Wrapper.ProxyValue; set { Wrapper.ProxyValue = value; OnPropertyChanged(); } }

        public RangeValueSetting(PropertyInfoWrapper wrapper)
        {
            InitializeComponent();

            Binding binding = new Binding();
            binding.Source = RefValue;
            binding.Mode = BindingMode.TwoWay;

            if (wrapper.PropertyInfo.GetCustomAttribute<RangeAttribute>() is RangeAttribute range)
            {
                binding.ConverterParameter = Wrapper;
                binding.Converter = GetValueConverter(wrapper.PropertyInfo.PropertyType, range);

                ValueRangeSlider.Minimum = double.Parse(range.Min);
                ValueRangeSlider.MaxHeight = double.Parse(range.Max);

                //确保slider的value和设置钦定的值一样
                if (!(wrapper.PropertyInfo.PropertyType.Name == "Single" || wrapper.PropertyInfo.PropertyType.Name == "Double"))
                {
                    ValueRangeSlider.IsSnapToTickEnabled = true;
                    ValueRangeSlider.TickFrequency = 1;
                }
            }
            else
            {
                throw new Exception("RangeValueSetting钦定设置属性必须有[Range]特性标识");
            }

            Input.SetBinding(TextBox.TextProperty, binding);
            ValueRangeSlider.SetBinding(Slider.ValueProperty, binding);

            if (wrapper.PropertyInfo.GetCustomAttribute<Settings.UIAttributes.DescriptionAttribute>() is Settings.UIAttributes.DescriptionAttribute description)
                NameBlock.ToolTip = description.Description;

            NameBlock.Name = wrapper.PropertyInfo.GetSettingPropDisplayName();
        }

        private static IValueConverter GetValueConverter(Type prop_type, RangeAttribute range)
        {
            switch (prop_type.Name)
            {
                case "Int32":
                    return new RangeValueConverter<int>(range);
                case "Single":
                    return new RangeValueConverter<float>(range);
                case "Int64":
                    return new RangeValueConverter<long>(range);
                case "Int16":
                    return new RangeValueConverter<short>(range);
                case "Byte":
                    return new RangeValueConverter<byte>(range);
                case "Double":
                    return new RangeValueConverter<double>(range);
                default:
                    throw new NotSupportedException();
            }
        }

        public void OnPropertyChanged([CallerMemberName] String prop_name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
