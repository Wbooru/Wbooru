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
using Wbooru.Utils;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Kernel;
using Wbooru.Models.SettingUI;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;
using Wbooru.UI.Controls.SettingUI;
using Wbooru.UI.ValueConverters.SettingUI;
using Wbooru.UI.Controls;
using System.Diagnostics;
using Wbooru.UI.Dialogs;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// SettingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page , INavigatableAction
    {
        public Type CurrentSettingType
        {
            get { return (Type)GetValue(CurrentSettingTypeProperty); }
            set { SetValue(CurrentSettingTypeProperty, value); }
        }

        public static readonly DependencyProperty CurrentSettingTypeProperty =
            DependencyProperty.Register("CurrentSettingType", typeof(Type), typeof(SettingPage), new PropertyMetadata(null));

        public class GroupedSupportSettingWrapper
        {
            public Assembly ReferenceAssembly { get; set; }
            public List<Type> SupportSettings { get; set; }
        }

        public IEnumerable<GroupedSupportSettingWrapper> SupportSettingWrappers
        {
            get { return (IEnumerable<GroupedSupportSettingWrapper>)GetValue(SupportSettingWrappersProperty); }
            set { SetValue(SupportSettingWrappersProperty, value); }
        }

        public static readonly DependencyProperty SupportSettingWrappersProperty =
            DependencyProperty.Register("SupportSettingWrappers", typeof(IEnumerable<GroupedSupportSettingWrapper>), typeof(SettingPage), new PropertyMetadata(default));

        private Dictionary<Type, FrameworkElement[]> cached_controls = new Dictionary<Type, FrameworkElement[]>();

        private Dictionary<PropertyInfoWrapper, int> record_hash = new Dictionary<PropertyInfoWrapper, int>();
        private Dictionary<Type, IEnumerable<PropertyInfoWrapper>> cached_records = new Dictionary<Type, IEnumerable<PropertyInfoWrapper>>();

        public SettingPage()
        {
            InitializeComponent();

            SupportSettingWrappers = Container.Default.GetExports<SettingBase>()
                .Select(x => SettingManager.LoadSetting(x.Value.GetType()))
                .OfType<SettingBase>()
                .GroupBy(x => x.GetType().Assembly)
                .Select(x => new GroupedSupportSettingWrapper() {
                    ReferenceAssembly = x.Key,
                    SupportSettings = x.AsEnumerable().Select(x => x.GetType()).ToList()
                });

            MainPanel.DataContext = this;

            ApplySetting(SupportSettingWrappers.First().SupportSettings.FirstOrDefault());
        }
        
        private void ApplySetting(Type setting_type)
        {
            if (setting_type == null)
                return;

            CurrentSettingType = setting_type;

            record_hash.Clear();

            if (!cached_controls.TryGetValue(setting_type, out var grouped_controls))
            {
                var props = setting_type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null);

                var need_restart_wrapper = props.Where(p => p.GetCustomAttribute<NeedRestartAttribute>() != null).Select(x => new PropertyInfoWrapper()
                {
                    PropertyInfo = x,
                    OwnerObject = SettingManager.LoadSetting(setting_type)
                }).ToArray();

                cached_records[setting_type] = need_restart_wrapper;

                Log.Debug($"Setting {CurrentSettingType.Name} has {record_hash.Count} props need restart.");

                var group_props = props.
                    Select(prop => (prop.GetCustomAttribute<GroupAttribute>()?.GroupName ?? "Other", new PropertyInfoWrapper()
                    {
                        OwnerObject = SettingManager.LoadSetting(setting_type),
                        PropertyInfo = prop
                    }))
                    .GroupBy(x => x.Item1);

                grouped_controls = group_props.Select(x => GenerateGroupedSettingControls(x)).OfType<FrameworkElement>().ToArray();

                //generate custom UI elements and append to group_controls.
                var groups = grouped_controls.OfType<GroupBox>().ToDictionary(x => x.Header.ToString(), x => x);

                TryAppendCustomUIElements(groups, setting_type);

                grouped_controls = groups.Values.ToArray();

                cached_controls[setting_type] = grouped_controls;
            }

            foreach (var wrapper in cached_records[setting_type])
                record_hash[wrapper] = wrapper.ProxyValue.GetHashCode();

            SettingListPanel.Children.Clear();

            foreach (var control in grouped_controls)
                SettingListPanel.Children.Add(control);
        }

        private void TryAppendCustomUIElements(Dictionary<string, GroupBox> groups, Type setting_type)
        {
            var generate_methods = setting_type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<CustomUIAttribute>() != null);

            var setting = SettingManager.LoadSetting(setting_type);

            foreach (var method in generate_methods)
            {
                if (!(method.Invoke(setting, new object[0]) is FrameworkElement ui))
                {
                    Log.Warn($"Can't generate custom ui from method : {setting_type.Name}.{method.Name}() , skip.");
                    continue;
                }

                var group_name = method.GetCustomAttribute<GroupAttribute>() is GroupAttribute group ? group.GroupName : "Other";

                Panel collection;

                if (groups.TryGetValue(group_name, out var group_control))
                {
                    collection = group_control.Content as Panel;
                }
                else
                {
                    Log.Info($"Generate new group {group_name} for {setting_type.Name}.{method.Name}()");

                    //add new group control
                    var group_box = new GroupBox();
                    var stack_panel = new StackPanel();

                    group_box.Content = stack_panel;
                    group_box.Header = group_name;
                    stack_panel.Margin = new Thickness(15, 0, 0, 0);

                    groups[group_name] = group_box;

                    collection = stack_panel;
                }

                if (collection != null)
                {
                    ui.Margin = new Thickness(0, 0, 0, 15);
                    collection?.Children?.Add(ui);

                    Log.Info($"Appended {setting_type.Name}.{method.Name}() custom control into group {group_name}");
                }
            }
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

                case "String":
                case "Double":
                case "Byte":
                case "Int16":
                case "UInt16":
                case "Int64":
                case "UInt64":
                case "Int32":
                case "UInt32":
                case "Single":
                    control = GenerateValueControl(wrapper);
                    break;

                default:
                    if (prop_info.PropertyType.IsEnum)
                        control = GenerateValueControl(wrapper);
                    else
                    {
                        Log.Warn($"Skip generate this setting prop {prop_info.Name} , because its type {prop_info.PropertyType.Name} is not supported.");
                        return null;
                    }
                    break;
            }

            #endregion

            #region Common Setup

            var control_name = $"GEN_{wrapper.OwnerObject.GetType().Name}_{wrapper.PropertyInfo.Name}_{control.GetType().Name}";
            control.Name = control_name;
            Log.Debug($"Create control:{control_name}");

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

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            OnNavigationBackAction();
        }

        public async Task CheckNeedRestartPropsAndNotify()
        {
            var changed_records = record_hash.Where(x => x.Key.ProxyValue.GetHashCode() != x.Value);

            Log.Debug($"record_hash=({record_hash.Count}) , there are {changed_records.Count()} props had been changed.");

            if (changed_records.Any() && !SettingManager.LoadSetting<GlobalSetting>().IgnoreSettingChangedComfirm)
                await Dialog.ShowDialog<SettingRestartComfirmDialog>();
        }

        private async void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var control = sender as FrameworkElement;
            var setting = control.DataContext as Type;

            if (setting.GetType() == CurrentSettingType)
                return;

            await CheckNeedRestartPropsAndNotify();
            ApplySetting(setting);
        }

        private void DefaultSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var setting_type = CurrentSettingType;

            SettingManager.ResetSetting(setting_type);

            cached_controls.Remove(setting_type);
            
            ApplySetting(setting_type);

            Toast.ShowMessage("重置成功!");
        }

        public async void OnNavigationBackAction()
        {
            await CheckNeedRestartPropsAndNotify();
            NavigationHelper.NavigationPop();
        }

        public void OnNavigationForwardAction()
        {

        }
    }
}
