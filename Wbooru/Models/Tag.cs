using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models
{
    public enum TagType
    {
        General,Artist, Character, Copyright,/* Ambiguous,*/Unknown,Circle,Faults
    }

    public class Tag
    {
        public string Name { get; set; }
        public TagType Type { get; set; }
    }
}
