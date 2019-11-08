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
        #region Tag Filter Options

        [Group("Tag Filter Options")]
        [Description("是否启用标签过滤，此功能会过滤指定的标签")]
        public bool EnableTagFilter { get; set; } = false;

        [Group("Tag Filter Options")]
        [List(typeof(TagFilterTarget), true, ",")]
        [EnableBy(nameof(EnableTagFilter))]
        [Description("需要标签过滤的目标")]
        public TagFilterTarget FilterTarget { get; set; } = TagFilterTarget.MainWindow | TagFilterTarget.SearchResultWindow;

        #endregion



        [Group("Tags Options")]
        [Description("是否预先下载且缓存标签数据集,此操作将会占用大量空间和网络带宽")]
        public bool PredownloadAndCacheTagData { get; set; } = false;

        [Group("Download Options")]
        [Path(false,false)]
        public string DownloadPath { get; set; } = "./Download";

        [Group("Download Options")]
        [Description("分别按照来源子文件夹来下载图片")]
        public bool SeparateGallerySubDirectories { get; set; } = true;

        [Group("View Options")]
        [Range("1","5")]
        public int LoadingImageThread { get; set; } = 2;

        [Group("View Options")]
        [Description("每次加载更多图片时所需要图片最低数量(多了的话可能会被ban?)")]
        public int GetPictureCountPerLoad { get; set; } = 20;

        [Group("View Options")]
        [NameAlias("是否使用滚轮或者拖曳滚动图片列表")]
        public bool GalleryListScrollBarVisiable { get; set; } = true;

        [Group("Test")]
        [NameAlias("Test alias name")]
        [List(true,false,",","maria","joe","tank","sb")]
        public string ListTestProp { get; set; } = "maria,sb";
    }
}
