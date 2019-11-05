using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryAccount
    {
        bool IsLoggined { get; }
        void AccountLogin();
        void AccountLogout();
    }
}
