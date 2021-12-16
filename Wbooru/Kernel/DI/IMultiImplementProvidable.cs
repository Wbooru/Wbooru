using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.DI
{
    /// <summary>
    /// 接口标记，表示实现这个的类有多个并可能被遍历使用
    /// 第三方可以通过实现这个类，以及使用MEF的Export特性标记
    /// 去添加新的实现
    /// </summary>
    public interface IMultiImplementProvidable
    {

    }
}
