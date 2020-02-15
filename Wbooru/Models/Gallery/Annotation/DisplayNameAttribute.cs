using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
