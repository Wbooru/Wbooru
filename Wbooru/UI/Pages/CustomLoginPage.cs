using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wbooru.Galleries.SupportFeatures;

namespace Wbooru.UI.Pages
{
    public abstract class CustomLoginPage : Page
    {
        public abstract AccountInfo AccountInfo { get; protected set; }
    }
}
