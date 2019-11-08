using LambdaConverters;
using System;
using System.Collections.Generic;
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
using Wbooru.Kernel;
using Wbooru.Models.SettingUI;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;
using Wbooru.UI.Controls.SettingUI;
using Wbooru.UI.ValueConverters.SettingUI;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// SettingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public class GroupedSupportSettingWrapper
        {
            public Assembly ReferenceAssembly { get; set; }
            public IEnumerable<SettingBase> SupportSettings { get; set; }
        }

        public IEnumerable<GroupedSupportSettingWrapper> SupportSettingWrappers
        {
            get { return (IEnumerable<GroupedSupportSettingWrapper>)GetValue(SupportSettingWrappersProperty); }
            set { SetValue(SupportSettingWrappersProperty, value); }
        }

        public static readonly DependencyProperty SupportSettingWrappersProperty =
            DependencyProperty.Register("SupportSettingWrappers", typeof(IEnumerable<GroupedSupportSettingWrapper>), typeof(SettingPage), new PropertyMetadata(default));

        private Dictionary<SettingBase, FrameworkElement[]> cached_controls = new Dictionary<SettingBase, FrameworkElement[]>();

        public SettingPage()
        {
            InitializeComponent();

            SupportSettingWrappers = Container.Default.GetExports<IUIVisualizable>()
                .Select(x=> SettingManager.LoadSetting(x.Value.GetType()))
                .OfType<SettingBase>()
                .GroupBy(x=>x.GetType().Assembly)
                .Select(x=>new GroupedSupportSettingWrapper() {
                    ReferenceAssembly=x.Key,
                    SupportSettings=x.AsEnumerable()
                });

            MainPanel.DataContext = this;

            ApplySetting(SupportSettingWrappers.First().SupportSettings.FirstOrDefault());
        }

        private void ApplySetting(SettingBase setting)
        {
            if (setting == null)
                return;

            if (!cached_controls.TryGetValue(setting, out var grouped_controls))
            {
                var props = setting.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var group_props = props.
                    Select(prop => (prop.GetCustomAttribute<GroupAttribute>()?.GroupName ?? "Other", new PropertyInfoWrapper()
                    {
                        OwnerObject = setting,
                        PropertyInfo = prop
                    }))
                    .GroupBy(x => x.Item1);

                grouped_controls = group_props.Select(x => GenerateGroupedSettingControls(x)).OfType<FrameworkElement>().ToArray();
                cached_controls[setting] = grouped_controls;
            }

            SettingListPanel.Children.Clear();

            foreach (var control in grouped_controls)
                SettingListPanel.Children.Add(control);
        }

        private FrameworkElement GenerateGroupedSettingControls(IGrouping<string, (string, PropertyInfoWrapper)> group_props)
        {
            var generated_setting_controls = group_props.Select(x => (GenerateMiniVisualizableSetting(x.Item2), x.Item2)).Where(x => x.Item1 != null).ToArray();

            if (generated_setting_controls.Count() == 0)
                return null;

            var group_box = new GroupBox();
            var stack_panel= new StackPanel();

            foreach (var (control,prop_info) in generated_setting_controls)
            {
                if (prop_info.PropertyInfo.GetCustomAttribute<EnableByAttribute>() is EnableByAttribute dep_attr)
                {
                    if (generated_setting_controls.FirstOrDefault(x=>x.Item2.PropertyInfo.Name.Equals(dep_attr.SettingName,StringComparison.InvariantCultureIgnoreCase)).Item1 is CheckBox dep_host)
                    {
                        var dp = dep_attr.HideIfDisable ? VisibilityProperty : IsEnabledProperty;
                        var conv = dep_attr.HideIfDisable ? ValueConverter.Create<bool?, Visibility>(e => e.Value ?? false ? Visibility.Visible : Visibility.Collapsed) : ValueConverter.Create<bool?, bool>(e => e.Value ?? false);

                        control.SetBinding(dp, new Binding()
                        {
                            Source = dep_host,
                            Mode= BindingMode.OneWay,
                            Path = new PropertyPath(nameof(CheckBox.IsChecked)),
                            Converter = conv
                        });

                        Log.Debug($"Bind a enable relation: {control.Name} -> {dep_host.Name}");
                    }
                }

                control.Margin = new Thickness(0, 0, 0, 15);
                stack_panel.Children.Add(control);
            }

            group_box.Content = stack_panel;
            group_box.Header = group_props.Key;
            stack_panel.Margin = new Thickness(15, 0, 0, 0);

            return group_box;
        }

        private FrameworkElement GenerateMiniVisualizableSetting(PropertyInfoWrapper wrapper)
        {
            FrameworkElement control = null;
            var prop_info = wrapper.PropertyInfo;

            #region Create Control

            switch (prop_info.PropertyType.Name)
            {
                case "Boolean":
                    control = GenerateBoolControl(wrapper);
                    break;

                case "Double":
                case "Byte":
                case "String":
                case "Int64":
                case "Int16":
                case "Int32":
                case "Single":
                    control = GenerateValueControl(wrapper);
                    break;

                default:
                    if (prop_info.PropertyType.IsEnum)
                        control = GenerateValueControl(wrapper);
                    else
                        return null;
                    break;
            }

            #endregion

            #region Common Setup

            var control_name = $"GEN_{wrapper.OwnerObject.GetType().Name}_{wrapper.PropertyInfo.Name}_{control.GetType().Name}";
            control.Name = control_name;
            Log.Debug($"Create control:{control_name}", "GenerateMiniVisualizableSetting");

            #endregion

            return control;
        }

        private FrameworkElement GenerateBoolControl(PropertyInfoWrapper wrapper)
        {
            var prop_info = wrapper.PropertyInfo;

            CheckBox checkBox = new CheckBox();

            checkBox.IsChecked = (bool)wrapper.ProxyValue;

            Binding binding = new Binding("ProxyValue");
            binding.Source = wrapper;
            binding.Mode = BindingMode.TwoWay;
            binding.Converter = new AutoValueConverter();
            binding.ConverterParameter = wrapper;

            checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);
            checkBox.Content = wrapper.DisplayPropertyName;

            if (wrapper.PropertyInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute description)
                checkBox.ToolTip = description.Description;

            return checkBox;
        }

        private FrameworkElement GenerateValueControl(PropertyInfoWrapper wrapper)
        {
            var prop_info = wrapper.PropertyInfo;
            FrameworkElement control = default;

            if (wrapper.PropertyInfo.GetCustomAttribute<RangeAttribute>() != null)
            {
                control = new RangeValueSetting(wrapper);
            }
            else if(wrapper.PropertyInfo.GetCustomAttribute<PathAttribute>() != null)
            {
                control = new PathSetting(wrapper);
            }
            else if (wrapper.PropertyInfo.GetCustomAttribute<ListAttribute>() != null)
            {
                control = new ListSetting(wrapper);
            }
            else
            {
                control = new CommonValueSetting(wrapper);
            }

            return control;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var control = (sender as FrameworkElement);
            ApplySetting(control.DataContext as SettingBase);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }
    }
}
