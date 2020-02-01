using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models
{
    public enum TagType
    {
        General=0, Artist=1, Character=4, Copyright=3,/* Ambiguous,*/Unknown=-2857, Circle=5, Faults=6
    }

    public class Tag
    {
        public string Name { get; set; }
        public TagType Type { get; set; }
    }
}
    