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
using Wbooru.Models.SettingUI;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;
using Wbooru.UI.Controls.SettingUI;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// SettingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPanel : Page
    {
        public IEnumerable<SettingBase> SupportSettings
        {
            get { return (IEnumerable<SettingBase>)GetValue(SupportSettingsProperty); }
            set { SetValue(SupportSettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SupportSettingsProperty =
            DependencyProperty.Register("SupportSettings", typeof(IEnumerable<SettingBase>), typeof(SettingPanel), new PropertyMetadata(Enumerable.Empty<IUIVisualizable>()));



        public SettingPanel()
        {
            InitializeComponent();

            SupportSettings = Container.Default.GetExports<IUIVisualizable>().Select(x=>x.Value).OfType<SettingBase>();
        }

        private void ApplySetting(SettingBase setting)
        {
            var props = setting.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var group_props = props.
                Select(prop => (prop.GetCustomAttribute<GroupAttribute>()?.GroupName??"Other",new PropertyInfoWrapper() {
                    OwnerObject=setting,
                    PropertyInfo=prop
                }))
                .GroupBy(x=>x.Item1);

            var grouped_controls = group_props.Select(x => GenerateGroupedSettingControls(x));
        }

        private FrameworkElement GenerateGroupedSettingControls(IGrouping<string, (string, PropertyInfoWrapper)> group_props)
        {
            var generated_setting_controls = group_props.Select(x => GenerateMiniVisualizableSetting(x.Item2));

            //todo
            return null;
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
                case "Int64":
                case "Int16":
                case "Int32":
                case "Single":
                    control = GenerateValueControl(wrapper);
                    break;

                default:
                    throw new NotSupportedException();
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

            checkBox.IsChecked = (bool)prop_info.GetValue(wrapper.OwnerObject);

            checkBox.Checked += (_,__) => prop_info.SetValue(wrapper.OwnerObject, true);
            checkBox.Unchecked += (_, __) => prop_info.SetValue(wrapper.OwnerObject, false);

            checkBox.Content = prop_info.GetSettingPropDisplayName();

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
            else
            {
                control = new CommonValueSetting(wrapper);
            }

            return control;
        }
    }
}
