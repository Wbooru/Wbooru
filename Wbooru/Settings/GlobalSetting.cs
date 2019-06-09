using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings.UIAttributes;

namespace Wbooru.Settings
{
    [Export(typeof(IUIVisualizable))]
    public class GlobalSetting : SettingBase , IUIVisualizable
    {
        public string DownloadPath { get; set; } = "./Download";

        [Group("View Options")]
        public int LoadingImageThread { get; set; } = 2;
        [Group("View Options")]
        public int GetPictureCountPerLoad { get; set; } = 20;
        [Group("View Options")]
        public bool GalleryListScrollBarVisiable { get; set; } = true;
    }
}
