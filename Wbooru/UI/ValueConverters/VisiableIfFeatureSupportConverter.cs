﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;

namespace Wbooru.UI.ValueConverters
{
    public class VisiableIfFeatureSupportConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var r =  (value as Gallery)?.SupportFeatures.HasFlag((GallerySupportFeature)Enum.Parse(typeof(GallerySupportFeature), parameter.ToString(),true)) ?? false ? Visibility.Visible : Visibility.Collapsed;
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
