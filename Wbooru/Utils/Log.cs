using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Wbooru
{
    public static class Log
    {
        private static StringBuilder sb = new StringBuilder(2048);

        private static ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;
        private static ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

        public static string BuildLogMessage(string message, bool new_line, bool time, string prefix)
        {
            sb.Clear();

            if (time)
            {
                var t = DateTime.Now;
                sb.AppendFormat("[{0} {1}]",t.ToShortDateString(),t.ToShortTimeString());
            }

            if(!string.IsNullOrWhiteSpace(prefix))
                sb.AppendFormat("{0}:", prefix);

            sb.AppendFormat("{0}:", message);

            if (new_line)
                sb.AppendLine();

            return sb.ToString();
        }

        internal static void Output(string message)
        {
            Console.Write(message);
        }

        internal static void ColorizeOutput(string message , ConsoleColor f,ConsoleColor b)
        {
            Console.BackgroundColor = b;
            Console.ForegroundColor = f;

            Output(message);

            Console.ResetColor();
        }

        public static void Info(string message, [CallerMemberName]string caller= "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, caller);
            Output(msg);
        }

        public static void Debug(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
#if DEBUG
            var msg = BuildLogMessage(message, true, true, caller);
            Output(msg);
#endif
        }

        public static void Warn(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, caller);
            ColorizeOutput(msg,ConsoleColor.Yellow,DefaultBackgroundColor);
        }

        public static void Error(string message, [CallerMemberName]string caller = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, caller);
            ColorizeOutput(msg, ConsoleColor.Red, ConsoleColor.Yellow);
        }
    }

    public static class Log<T>
    {
        public static readonly string PREFIX = typeof(T).Name;

        public static void Info(string message) => Log.Info(message, PREFIX);
        public static void Debug(string message) => Log.Debug(message, PREFIX);
        public static void Warn(string message) => Log.Warn(message, PREFIX);
        public static void Error(string message) => Log.Error(message, PREFIX);
    }
}
