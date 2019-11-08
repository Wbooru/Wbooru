using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries
{
    public interface IContainDetail
    {
        GalleryImageDetail GalleryDetail { get; set; }
    }
}
