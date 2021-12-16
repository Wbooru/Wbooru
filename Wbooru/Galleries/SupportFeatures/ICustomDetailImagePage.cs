using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.UI.Pages;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface ICustomDetailImagePage : IGalleryFeature
    {
        DetailImagePageBase GenerateDetailImagePageObject();
    }
}
