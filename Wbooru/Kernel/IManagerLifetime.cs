using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel
{
    public interface IManagerLifetime
    {
        Task OnInit();
        Task OnExit();
    }
}
