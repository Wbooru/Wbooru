using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wbooru.UI.Controls;

namespace Wbooru.Utils
{
    public static class ExceptionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugThrow(Exception e)
        {
#if DEBUG
            throw new AggregateException("DebugThrow", e);
#endif

#pragma warning disable CS0162
            Log.Error("Caught a exception but program needn't exit:" + e.Message + Environment.NewLine + e.StackTrace);
#pragma warning restore CS0162
        }

        public static void ToastNotice(Exception e)
        {
            DebugThrow(e);
            Toast.ShowMessage(e.Message);
        }
    }
}
