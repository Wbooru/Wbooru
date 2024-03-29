﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Models
{
    public class VisitRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int VisitRecordID { get; set; }

        public virtual GalleryItem GalleryItem { get; set; }
        public DateTime LastVisitTime { get; set; }
    }
}
