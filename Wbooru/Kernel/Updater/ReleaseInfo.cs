using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.Updater
{
    public class ReleaseInfo
    {
        public ReleaseType ReleaseType { get; set; }
        public Version Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string DownloadURL { get; set; }
        public string ReleaseURL { get; set; }
        public string ReleaseDescription { get; set; }
    }
}
