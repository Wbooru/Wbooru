using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Persistence
{
    public class Tag
    {
        public int TagID { get; set; }

        public string TagName { get; set; }
        public DateTime AddTime { get; set; }
        public string FromGallery { get; set; }
    }
}
