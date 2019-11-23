using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Wbooru.Settings;
using System.IO;
using Wbooru.Utils;

namespace Wbooru
{
    public static class Log
    {
#if DEBUG
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);
#endif

        private static StringBuilder sb = new StringBuilder(2048);

        private static ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;
        private static ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

        private static string log_file_path;
        private static StreamWriter file_writer; 

        static Log()
        {
            var log_dir =Path.GetFullPath(SettingManager.LoadSetting<GlobalSetting>().LogOutputDirectory);
            Directory.CreateDirectory(log_dir);
            log_file_path = Path.Combine(log_dir, FileNameHelper.FilterFileName(DateTime.Now.ToString() + ".log", '-'));

            file_writer = File.AppendText(log_file_path);
            file_writer.AutoFlush = true;

            Info($"log_file_path = {log_file_path}");
        }

        private static void FileWrite(string content)
        {
            if (file_writer!=null)
            {
                file_writer.Write(content);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(log_file_path))
                    File.AppendAllText(log_file_path, content);
            }
        }

        public static void Term()
        {
            file_writer.Flush();
            file_writer.Close();
        }

        public static string BuildLogMessage(string message, bool new_line, bool time, string prefix)
        {
            lock (sb)
            {
                sb.Clear();

                if (time)
                {
                    var t = DateTime.Now;
                    sb.AppendFormat("[{0} {1}]", t.ToShortDateString(), t.ToShortTimeString());
                }

                if (!string.IsNullOrWhiteSpace(prefix))
                    sb.AppendFormat("{0}", prefix);

                sb.AppendFormat(":{0}", message);

                if (new_line)
                    sb.AppendLine();

                return sb.ToString();
            }
        }

        internal static void Output(string message)
        {
#if DEBUG
            OutputDebugString(message);
#endif
            Console.Write(message);
            FileWrite(message);
        }

        internal static void ColorizeOutput(string message , ConsoleColor f,ConsoleColor b)
        {
            Console.BackgroundColor = b;
            Console.ForegroundColor = f;

            Output(message);

            Console.ResetColor();
        }

        public static void Info(string message, [CallerMemberName]string prefix = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, prefix);
            Output(msg);
        }

        public static void Debug(string message, [CallerMemberName]string prefix = "<Unknown Method>")
        {
#if DEBUG
            var msg = BuildLogMessage(message, true, true, prefix);
            Output(msg);
#endif
        }

        public static void Warn(string message, [CallerMemberName]string prefix = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, prefix);
            ColorizeOutput(msg,ConsoleColor.Yellow,DefaultBackgroundColor);
        }

        public static void Error(string message, [CallerMemberName]string prefix = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, prefix);
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
