using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{
    /// <summary>
    /// 表示在主页面中，画廊能提供快速跳过部分图片的功能
    /// 如果不实现此接口，那么将会对Gallery::GetMainPostedImages()的遍历使用Skip()，但也可能因此造成大量的请求以及长时间的处理.
    /// </summary>
    public interface IGalleryItemIteratorFastSkipable : IGalleryFeature
    {
        IEnumerable<GalleryItem> IteratorSkip(int skip_count);
    }
}
