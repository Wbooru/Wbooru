using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Data
    {
        [Required]
        public string Name { get; set; }

        [Key]
        public int UserID { get; set; }
    }
}
