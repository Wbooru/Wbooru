using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wbooru.Utils
{
    public class LogicalTreeHelperEx
    {
        public static IEnumerable<T> GetAllRecursively<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj is T t)
                yield return t;
            foreach (var i in LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>().Select(x => GetAllRecursively<T>(x)).SelectMany(x => x))
                yield return i;
        }
    }
}
