using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wbooru.Galleries;
using Wbooru.Models.Gallery;

namespace Wbooru.UI.Pages
{
    public abstract class DetailImagePageBase : Page
    {
       public abstract void ApplyItem(Gallery gallery, GalleryItem item);
    }
}
