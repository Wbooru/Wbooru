using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.DI
{
    /// <summary>
    /// 接口标记，表示实现这个的类是可以被替换使用的
    /// 第三方可以通过实现这个类，以及使用MEF的Export/PriorityExport特性标记
    /// 去钦定要替换默认实现的接口
    /// </summary>
    public interface IImplementInjectable
    {

    }
}
