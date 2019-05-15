using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public static class ExceptionHelper
    {
        public static void DebugThrow(Exception e)
        {
#if DEBUG
            throw e;
#endif
        }
    }
}
