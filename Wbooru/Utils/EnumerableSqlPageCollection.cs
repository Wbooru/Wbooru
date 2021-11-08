using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Wbooru.Utils
{
    /// <summary>
    /// 通过Sql的分页兼容实现IEnumerable枚举
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableSqlPageCollection<T> : IEnumerable<T>
    {
        Func<IQueryable<T>> queryableGenerator;

        public EnumerableSqlPageCollection(Func<IQueryable<T>> queryableGenerator)
        {
            this.queryableGenerator = queryableGenerator;
        }

        private class Enumerator : IEnumerable<T>
        {
            int count = 0;
            bool isDone = false;
            const int PageSize = 20;
            IQueryable<T> queryable;

            public Enumerator(IQueryable<T> queryable)
            {
                this.queryable = queryable;
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            public IEnumerator<T> GetEnumerator()
            {
                while (!isDone)
                {
                    var result = queryable.Skip(count).Take(PageSize).ToArray();
                    count += result.Length;
                    isDone = result.Length == 0;

                    foreach (var item in result)
                    {
                        yield return item;
                    }

                    Log.Info($"=============> taken {count} objs");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(queryableGenerator()).GetEnumerator();
        }

        public IEnumerable<T> GetEnumerable()
        {
            return this;
        }
    }
}
