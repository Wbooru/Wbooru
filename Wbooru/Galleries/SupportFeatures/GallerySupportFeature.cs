using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Galleries.SupportFeatures
{
    [Flags]
    public enum GallerySupportFeature : ulong
    {
        Vote = 2 << 1,
        /// <summary>
        /// 若不支持将会使用本地数据库的标记功能
        /// </summary>
        Mark = 2 << 2,
        Account = 2 << 3,
        TagSearch = 2 << 4,
        TagMetaSearch = 2 << 5,
        ImageSearch = 2 << 6,
        ImageFastSkipable = 2 << 7,
        NSFWFilter = 2 << 8,
        CustomDetailImagePage = 2 << 9,
    }
}
