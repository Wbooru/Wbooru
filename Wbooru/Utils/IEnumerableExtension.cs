using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru
{
    public static class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> collection,Action<T> action)
        {
            foreach (var item in collection)
                action.Invoke(item);
        }

        public static IEnumerable<IEnumerable<T>> SequenceWrap<T>(this IEnumerable<T> collection,int wrapCount)
        {
            var i = 0;

            var arr = ArrayPool<T>.Shared.Rent(wrapCount);

            foreach (var item in collection)
            {
                arr[i++] = item;

                if (i == wrapCount)
                {
                    yield return arr.Take(wrapCount);
                    i = 0;
                }
            }

            if (i != 0)
                yield return arr.Take(i).ToArray();

            ArrayPool<T>.Shared.Return(arr);
        }
    }
}
