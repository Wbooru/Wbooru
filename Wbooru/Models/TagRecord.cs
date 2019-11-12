using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;

namespace Wbooru.Models
{
    public class TagRecord
    {
        [Key]
        public int TagID { get; set; }

        public Tag Tag { get; set; }
        public DateTime AddTime { get; set; }
        public string FromGallery { get; set; }

        public TagRecordType RecordType { get; set; }

        [Flags]
        public enum TagRecordType
        {
            Filter = 2 << 1,
            Marked = 2 << 2,
            Subscribed = Marked | 2 << 3
        }
    }
}
