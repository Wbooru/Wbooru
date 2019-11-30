using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel
{
    /// <summary>
    /// 支持各种导航按键功能
    /// </summary>
    public interface INavigatableAction
    {
        public void OnNavigationBackAction();
        public void OnNavigationForwardAction();
    }
}
