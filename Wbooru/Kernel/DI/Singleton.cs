using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.DI
{
    public class Singleton<T> where T : class
    {
        public static T Instance => Container.Get<T>();
    }
}
