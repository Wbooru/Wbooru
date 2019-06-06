using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public static class MathEx
    {
        public static T Max<T>(T a,T b) where T : IComparable
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

        public static T Min<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) < 0 ? a : b;
        }
    }
}
