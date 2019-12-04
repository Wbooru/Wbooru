using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public class Option
    {
        public string Name { get; set; }

        public override string ToString() => $"{Name}";
    }

    public class ValueOption:Option 
    {
        public string Value { get; set; }

        public override string ToString() => $"{base.ToString()} = {Value}";
    }


    //xx.exe -f=123 -o = "name" -o = "56467" -p 
    public static class CommandLine
    {
        static IEnumerable<Option> cached_options;

        static CommandLine()
        {
            cached_options = Environment.GetCommandLineArgs().Select(x => Regex.Match(x, @"-([a-zA-Z0-9_]\w*)(=(.+))?"))
                .Where(x => x.Success)
                .Select(x =>
                {
                    var name = x.Groups[1].Value.Trim();
                    var val = x.Groups[3].Value.Trim();

                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        return new ValueOption()
                        {
                            Name = name,
                            Value = val
                        };
                    }
                    else
                    {
                        return new Option()
                        {
                            Name = name
                        };
                    }
                }).ToArray();

            foreach (var options in cached_options)
                Log.Debug(options.ToString(), "CommandLineParseResult");
        }

        public static IEnumerable<Option> SwitchOptions => cached_options.Where(x => !(x is ValueOption));
        public static IEnumerable<ValueOption> ValueOptions => cached_options.OfType<ValueOption>();

        public static bool TryGetOptionValue<T>(string name, out T val)
        {
            val = default;

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));

                if (ValueOptions.FirstOrDefault(x => x.Name == name) is ValueOption option)
                {
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        val = (T)converter.ConvertFrom(option.Value);
                        return true;
                    }
                }

                return false;

            }
            catch (Exception e)
            {
                Log.Error($"Can't get/convert value from command line parser : " + e.Message);
                return false;
            }
        }

        public static bool ContainSwitchOption(string name) => SwitchOptions.Any(x => x.Name == name);
    }
}
