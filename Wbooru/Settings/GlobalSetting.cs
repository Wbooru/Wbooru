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
        [Group("Download Options")]
        [Path(false,false)]
        public string DownloadPath { get; set; } = "./Download";

        [Group("View Options")]
        [Range("1","5")]
        public int LoadingImageThread { get; set; } = 2;

        [Group("View Options")]
        [Description("每次加载更多图片时所需要图片最低数量(多了的话可能会被ban?)")]
        public int GetPictureCountPerLoad { get; set; } = 20;

        [Group("View Options")]
        [NameAlias("ShowGalleryScrollBar")]
        public bool GalleryListScrollBarVisiable { get; set; } = true;

        [Group("Test")]
        [NameAlias("Test alias name")]
        [List(true,true,false,",","maria","joe","tank","sb")]
        public string ListTestProp { get; set; } = "maria,sb";
    }
}
