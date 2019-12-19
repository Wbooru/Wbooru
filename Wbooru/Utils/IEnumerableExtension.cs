using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public static class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> collection,Action<T> action)
        {
            foreach (var item in collection)
                action.Invoke(item);
        }
    }
}
