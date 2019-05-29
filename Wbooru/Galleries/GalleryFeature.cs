using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Galleries
{
    [Flags]
    public enum GalleryFeature
    {
        Vote = 2,
        Mark = 4,
        Login = 8,
        Logout = 16
    }
}
