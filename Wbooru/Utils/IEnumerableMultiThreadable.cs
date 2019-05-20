using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public static class IEnumerableMultiThreadableExtention
    {
        public static IEnumerable<T> MakeMultiThreadable<T>(this IEnumerable<T> source)
        {
            return new CacheEnumerator<T>(source);
        }

        private class CacheEnumerator<T> : IEnumerable<T>
        {
            private CacheEntry<T> cacheEntry;
            public CacheEnumerator(IEnumerable<T> sequence)
            {
                cacheEntry = new CacheEntry<T>();
                cacheEntry.Sequence = sequence.GetEnumerator();
                cacheEntry.CachedValues = new List<T>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (cacheEntry.FullyPopulated)
                    return cacheEntry.CachedValues.GetEnumerator();
                else
                    return iterateSequence<T>(cacheEntry).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private static IEnumerable<T> iterateSequence<T>(CacheEntry<T> entry)
        {
            for (int i = 0; entry.ensureItemAt(i); i++)
            {
                yield return entry.CachedValues[i];
            }
        }

        private class CacheEntry<T>
        {
            public bool FullyPopulated { get; private set; }
            public IEnumerator<T> Sequence { get; set; }

            public List<T> CachedValues { get; set; }

            public bool ensureItemAt(int index)
            {
                lock (this)
                {
                    if (index < CachedValues.Count)
                        return true;
                    if (FullyPopulated)
                        return false;

                    if (Sequence.MoveNext())
                    {
                        CachedValues.Add(Sequence.Current);
                        return true;
                    }
                    else
                    {
                        Sequence.Dispose();
                        FullyPopulated = true;
                        return false;
                    }
                }
            }
        }
    }
}
