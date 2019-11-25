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
using static Wbooru.Settings.GlobalSetting;

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

        public static string LogFilePath { get; private set; }

        private static StreamWriter file_writer;
        private static bool enable_debug_output;

        static Log()
        {
            var log_dir = SettingManager.LoadSetting<GlobalSetting>().LogOutputDirectory;
#if !DEBUG
            enable_debug_output = SettingManager.LoadSetting<GlobalSetting>().EnableOutputDebugMessage;
#else
            enable_debug_output = true;
#endif

            var console_window_option = SettingManager.LoadSetting<GlobalSetting>().ShowOutputWindow;
            var enable_show_console_window = console_window_option == LogWindowShowOption.None ? false : (console_window_option == LogWindowShowOption.Always ? true : enable_debug_output);

            if (enable_show_console_window)
                ConsoleWindow.Show();

            Directory.CreateDirectory(log_dir);
            LogFilePath = Path.Combine(log_dir, FileNameHelper.FilterFileName(DateTime.Now.ToString() + ".log", '-'));

            file_writer = File.AppendText(LogFilePath);
            file_writer.AutoFlush = true;

            Info($"log_file_path = {LogFilePath}");
        }

        private static void FileWrite(string content)
        {
            if (file_writer!=null)
            {
                file_writer.Write(content);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(LogFilePath))
                    File.AppendAllText(LogFilePath, content);
            }
        }

        public static void Term()
        {
            var s = file_writer;
            file_writer = null;
            s.Flush();
            s.Close();
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

        internal static void ColorizeConsoleOutput(string message , ConsoleColor f,ConsoleColor b)
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
            if (enable_debug_output)
            {
                var msg = BuildLogMessage(message, true, true, prefix);
                Output(msg);
            }
        }

        public static void Warn(string message, [CallerMemberName]string prefix = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, prefix);
            ColorizeConsoleOutput(msg,ConsoleColor.Yellow,DefaultBackgroundColor);
        }

        public static void Error(string message, [CallerMemberName]string prefix = "<Unknown Method>")
        {
            var msg = BuildLogMessage(message, true, true, prefix);
            ColorizeConsoleOutput(msg, ConsoleColor.Red, ConsoleColor.Yellow);
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
