using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.DI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class PriorityExport : ExportAttribute , IPriorityMetadata
    {
        public const uint WBOORU_DEFAULT_PRIORITY = uint.MinValue;

        public uint Priority { get; set; } = WBOORU_DEFAULT_PRIORITY;

        public PriorityExport(Type contractType) : base(contractType)
        {

        }
    }
}
