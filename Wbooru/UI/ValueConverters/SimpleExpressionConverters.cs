using LambdaConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Wbooru.Galleries.SupportFeatures;

namespace Wbooru.UI.ValueConverters
{
    public static class SimpleExpressionConverters
    {
        public static IValueConverter ReverseBooleanConverter => ValueConverter.Create<bool, bool>(x => !x.Value);
        public static IValueConverter ReverseBooleanToVisibilityConverter => ValueConverter.Create<bool?, Visibility>(b => !(b.Value ?? false) ? Visibility.Visible : Visibility.Hidden);
        public static IValueConverter BooleanToVisibilityConverter => ValueConverter.Create<bool?, Visibility>(b => (b.Value ?? false) ? Visibility.Visible : Visibility.Hidden);
    }
}
