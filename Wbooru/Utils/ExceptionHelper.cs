﻿using System;
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
            throw e;
#endif
        }

        public static void ToastNotice(Exception e)
        {
            DebugThrow(e);
            Container.Default.GetExportedValue<Toast>().ShowMessage(e.Message);
        }
    }
}
