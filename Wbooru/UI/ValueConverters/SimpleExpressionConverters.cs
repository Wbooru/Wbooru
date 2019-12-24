using LambdaConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Kernel.Updater.PluginMarket;
using Wbooru.PluginExt;
using Wbooru.Settings;

namespace Wbooru.UI.ValueConverters
{
    public static class SimpleExpressionConverters
    {
        public static IValueConverter ReverseBooleanConverter => ValueConverter.Create<bool, bool>(x => !x.Value);
        public static IValueConverter ReverseBooleanToVisibilityConverter => ValueConverter.Create<bool?, Visibility>(b => !(b.Value ?? false) ? Visibility.Visible : Visibility.Hidden);
        public static IValueConverter BooleanToVisibilityConverter => ValueConverter.Create<bool?, Visibility>(b => (b.Value ?? false) ? Visibility.Visible : Visibility.Hidden);
        public static IValueConverter LoginStatusStringConverter => ValueConverter.Create<bool, string>(b => (b.Value) ? "登出" : "登录");

        public static IValueConverter NullToBooleanConverter => ValueConverter.Create<object, bool>(
            b => {
                Log.Debug("{b.Value}", nameof(NullToBooleanConverter));
                return b.Value != null;
            });

        public static IValueConverter LatestUpdateDateConverter => ValueConverter.Create<IEnumerable<PluginMarketRelease>, DateTime?>(post => {
            if (post.Value is IEnumerable<PluginMarketRelease> list && list.Count() != 0)
            {
                return list.Max(x => x.ReleaseDate);
            }
            else
            {
                return default;
            }
        });

        public static IValueConverter ReverseCheckIfPluginInstalled => ValueConverter.Create<PluginMarketPost, Visibility>(
            x => !Container.Default.GetExportedValues<PluginInfo>().Any(y => y.PluginName.Equals(x.Value?.PluginName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)) ? Visibility.Visible : Visibility.Collapsed);
        public static IValueConverter CheckIfPluginInstalled => ValueConverter.Create<PluginMarketPost, Visibility>(
            x => Container.Default.GetExportedValues<PluginInfo>().Any(y => y.PluginName.Equals(x.Value?.PluginName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)) ? Visibility.Visible : Visibility.Collapsed);

        public static IValueConverter IfStringEmptyOrNullConverter => ValueConverter.Create<string, bool>(x => string.IsNullOrEmpty(x.Value));

        public static IValueConverter VisibilityIFNSFWModeConverter => ValueConverter.Create<object, Visibility>(_ =>
        {
            var r = SettingManager.LoadSetting<GlobalSetting>().EnableNSFWFileterMode ? Visibility.Visible : Visibility.Collapsed;
            return r;
        });
    }
}
