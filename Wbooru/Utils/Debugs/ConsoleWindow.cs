using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Wbooru.Utils.Debugs.ConsoleWindow.Command;

namespace Wbooru.Utils.Debugs
{
    public static class ConsoleWindow
    {
        public class Command
        {
            public class CommandArg
            {
                public string Name { get; set; }
                public string Value { get; set; }

                public override string ToString() => $"{Name} = {Value}";
            }

            public string Name { get; set; }
            public IEnumerable<CommandArg> Args { get; set; }

            public override string ToString() => $"{Name} | {string.Join(",", Args)}";
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern int GetConsoleOutputCP();

        private const int MF_BYCOMMAND = 0x00000000;

        private const int SC_CLOSE = 0xF060;

        private static Regex regex = new Regex(@"-([a-zA-Z0-9_]\w*)(=(.+))?");

        private static AbortableThread consoleInputThread;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        public delegate bool ProcessConsoleInputCommandFunc(Command command);

        public static event ProcessConsoleInputCommandFunc OnProcessConsoleInputCommandEvent;

        static ConsoleWindow()
        {
            ConsoleCommandProcessor.InitDefault();
        }

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            //#if DEBUG
            if (!HasConsole)
            {
                AllocConsole();
                DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
                InvalidateOutAndError();

                consoleInputThread?.Abort();
                consoleInputThread = new AbortableThread(ct =>
                {
                    var input = "";
                    while (!ct.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                        var stream = new StreamReader(Console.OpenStandardInput());
                        while (stream.Peek() != -1)
                        {
                            var c = (char)stream.Read();
                            if (c == '\n')
                            {
                                //post console command
                                OnProcessCommandInternal(input);
                                input = "";
                            }
                            else if (c != '\r')
                            {
                                input += c;
                            }
                        }
                    }
                });
                consoleInputThread.Start();
            }
            //#endif
        }

        private static void OnProcessCommandInternal(string input)
        {
            var splits = input.Split(' ');
            var commandName = splits[0];
            var optionsLine = string.Join(" ", splits.Skip(1));

            var options = regex.Matches(optionsLine)
                .Select(x =>
                {
                    var name = x.Groups[1].Value.Trim();
                    var val = x.Groups[3].Value.Trim();

                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        return new CommandArg
                        {
                            Name = name,
                            Value = val
                        };
                    }
                    else
                    {
                        return new CommandArg
                        {
                            Name = name,
                            Value = ""
                        };
                    }
                }).ToArray();

            var command = new Command()
            {
                Name = commandName,
                Args = options
            };

            OnProcessConsoleInputCommandEvent?.Invoke(command);
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            //#if DEBUG
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
            //#endif
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            Type type = typeof(Console);

            System.Reflection.FieldInfo _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic) ?? type.GetField("s_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.FieldInfo _error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic) ?? type.GetField("s_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            _out?.SetValue(null, null);
            _error?.SetValue(null, null);
        }

        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}
