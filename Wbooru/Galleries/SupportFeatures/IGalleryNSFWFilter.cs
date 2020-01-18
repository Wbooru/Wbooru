using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{

    /*
     * 表示这个图源支持NSFW过滤功能，在读取图片列表时能自主过滤那些不适宜公开浏览图片。
     * 一般来说Wbooru会自行调用NSFWFilter()方法来过滤
     * 但建议获取列表的方法内部自己也自主判断过滤。
     */

    public interface IGalleryNSFWFilter : IGalleryFeature
    {
        IAsyncEnumerable<GalleryItem> NSFWFilter(IAsyncEnumerable<GalleryItem> items);
    }
}
