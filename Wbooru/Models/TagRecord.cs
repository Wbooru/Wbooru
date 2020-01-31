using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagID { get; set; }

        public Tag Tag { get; set; }
        public DateTime AddTime { get; set; }
        public string FromGallery { get; set; }

        public TagRecordType RecordType { get; set; }

        [Flags]
        public enum TagRecordType
        {
            None = 0,//for tag type cache
            Filter = 2 << 1,
            Marked = 2 << 2,
            Subscribed = Marked | 2 << 3
        }
    }
}
