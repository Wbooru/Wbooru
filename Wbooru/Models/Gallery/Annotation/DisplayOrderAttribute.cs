using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DisplayOrderAttribute:Attribute
    {
        public DisplayOrderAttribute(int order = 0)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
