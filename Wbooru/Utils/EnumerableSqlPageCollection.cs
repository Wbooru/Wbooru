using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Persistence;

namespace Wbooru.Utils
{
    /// <summary>
    /// 通过Sql的分页兼容实现IEnumerable枚举
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableSqlPageCollection<T> : IAsyncEnumerable<T>
    {
        private readonly Func<LocalDBContext, IQueryable<T>> queryableGenerator;

        public EnumerableSqlPageCollection(Func<LocalDBContext, IQueryable<T>> queryableGenerator)
        {
            this.queryableGenerator = queryableGenerator;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            int count = 0;
            bool isDone = false;
            const int PageSize = 20;

            while (!isDone)
            {
                var result = await LocalDBContext.PostDbAction(ctx =>
                {
                    return queryableGenerator(ctx).Skip(count).Take(PageSize).ToArray();
                });
                count += result.Length;
                isDone = result.Length == 0;

                foreach (var item in result)
                    yield return item;

                Log.Debug($"taken {count} objs , isDone : {isDone}");
            }
        }
    }
}
