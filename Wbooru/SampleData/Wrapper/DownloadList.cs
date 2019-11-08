using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;

namespace Wbooru.SampleData.Wrapper
{
    public class DownloadList
    {
        IEnumerable<Download> Downloads { get; set; }
    }
}
