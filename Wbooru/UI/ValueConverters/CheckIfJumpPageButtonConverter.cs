using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Settings;
using static Wbooru.UI.Pages.MainGalleryPage;

namespace Wbooru.UI.ValueConverters
{
    public class CheckIfJumpPageButtonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var avaliable = 
                //判断是否为主页面且画廊支持快速翻页功能
                ((values.First() as Gallery).SupportFeatures.HasFlag(GallerySupportFeature.ImageFastSkipable) && ((GalleryViewType)values.Last()) == GalleryViewType.Main) 
                //判断是否强制开启此功能
                || SettingManager.LoadSetting<GlobalSetting>().ForceEnablePageJumpFeature;

            return avaliable ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
