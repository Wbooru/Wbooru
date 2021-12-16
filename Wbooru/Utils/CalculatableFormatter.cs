using ExtrameFunctionCalculator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public class CalculatableFormatter
    {
        Calculator calculator;
        Regex reg = new Regex(@"\$\{(.*?)\}");

        public CalculatableFormatter()
        {
            calculator = new Calculator();
        }

        public string Calculate(string expression)
        {
            return calculator.Solve(expression);
        }

        public string FormatCalculatableString(string content,Func<string,string> request_val=null)
        {
            var calc_expr_list =
                reg.Matches(content)
                .OfType<Match>()
                .Select(x => (x.Groups[0].Value,Calculate(x.Groups[1].Value)));


            foreach (var x in calc_expr_list)
            {
                content=content.Replace(x.Value, x.Item2);
            }

            return content;
        }
    }
}
