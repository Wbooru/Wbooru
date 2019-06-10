using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
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
using System.Reflection;
using Wbooru.UI.ValueConverters.SettingUI;

namespace Wbooru.UI.Controls.SettingUI
{
    /// <summary>
    /// ListSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ListSetting : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public PropertyInfoWrapper Wrapper { get; }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ListSetting), new PropertyMetadata(""));

        public ObservableCollection<string> ComboBoxSelectItems { get; set; }

        public ListAttribute PropListAttribute
        {
            get { return (ListAttribute)GetValue(PropListAttributeProperty); }
            set { SetValue(PropListAttributeProperty, value); }
        }

        public static readonly DependencyProperty PropListAttributeProperty =
            DependencyProperty.Register("PropListAttribute", typeof(ListAttribute), typeof(ListSetting), new PropertyMetadata(default
           ));

        public ListSetting()
        {
            InitializeComponent();

            MainComboBox.DataContext = this;
        }

        public ListSetting(PropertyInfoWrapper wrapper)
        {
            InitializeComponent();

            MainPanel.DataContext = this;

            Wrapper = wrapper;

            if (!(wrapper.PropertyInfo.GetCustomAttribute<ListAttribute>() is ListAttribute list_attr))
                throw new Exception("ListSetting钦定设置属性必须有[List]特性标识");

            PropListAttribute = list_attr;

            GenerateComboBoxItems();

            //create binding for text

            Binding binding = new Binding();
            binding.Source = Wrapper;
            binding.Path = new PropertyPath("ProxyValue");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new AutoValueConverter();
            binding.ConverterParameter = Wrapper;

            SetBinding(ListSetting.TextProperty, binding);

            Text = wrapper.ProxyValue.ToString();
        }

        private void GenerateComboBoxItems()
        {
            var cur_values = Wrapper.ProxyValue.ToString()
                .Split(new[] { PropListAttribute.SplitContent }, StringSplitOptions.RemoveEmptyEntries)
                .Intersect(PropListAttribute.Values);
            var value_controls = PropListAttribute.Values
                .Select(x=>PropListAttribute.MultiSelect? GenerateCheckbox(x,cur_values.Any(y=>y==x)):GenerateTextblock(x));

            MainComboBox.Items.Clear();

            foreach (var control in value_controls)
            {
                MainComboBox.Items.Add(control);
            }
        }

        private FrameworkElement GenerateCheckbox(string x,bool is_selected)
        {
            var control = new CheckBox();

            control.IsChecked = is_selected;
            control.Content = x;

            control.Checked += (_, __) => UpdateMultiSelectResult();
            control.Unchecked += (_, __) => UpdateMultiSelectResult();

            return control;
        }

        private void UpdateMultiSelectResult()
        {
            var update_result = string.Join(PropListAttribute.SplitContent, MainComboBox.Items
                .OfType<CheckBox>()
                .Where(x => x.IsChecked ?? false)
                .Select(x => x.Content.ToString()));

            Text = update_result;
        }

        private FrameworkElement GenerateTextblock(string x)
        {
            return new TextBlock()
            {
                Text=x
            };
        }
    }
}
