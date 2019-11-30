using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wbooru.Settings.UIAttributes;

namespace Wbooru.Settings
{
    [Export(typeof(SettingBase))]
    public class GlobalSetting : SettingBase
    {
        #region Tag Filter Options

        [Group("Tag Filter Options")]
        [Description("是否启用标签过滤，此功能会过滤指定的标签")]
        public bool EnableTagFilter { get; set; } = false;

        [Group("Tag Filter Options")]
        [List(typeof(TagFilterTarget), true)]
        [EnableBy(nameof(EnableTagFilter),true)]
        [Description("需要标签过滤的目标")]
        public TagFilterTarget FilterTarget { get; set; } = TagFilterTarget.MainWindow | TagFilterTarget.SearchResultWindow;

        [Group("Tag Filter Options")]
        [EnableBy(nameof(EnableTagFilter), true)]
        [Description("过滤列表是否共用")]
        public bool UseAllGalleryFilterList { get; set; } = false;

        #endregion

        #region Tags Options

        [Group("Tags Options")]
        [Description("是否预先下载且缓存标签数据集,此操作将会占用大量空间和网络带宽")]
        public bool PredownloadAndCacheTagData { get; set; } = false;


        [Group("Tags Options")]
        [Description("搜索图片标签时，标签建议最大数量,0代表不限制")]
        [Range(0,1000)]
        public int MaxSearchSuggestsCount { get; set; } = 100;

        #endregion

        #region Download Options

        [Group("Download Options")]
        [Path(false,false)]
        public string DownloadPath { get; set; } = "./Download";

        [Group("Download Options")]
        [Description("分别按照来源子文件夹来下载图片")]
        public bool SeparateGallerySubDirectories { get; set; } = true;

        #endregion

        #region View Options

        [NeedRestart]
        [Group("View Options")]
        [Range(1,5)]
        public int LoadingImageThread { get; set; } = 2;

        [NeedRestart]
        [Group("View Options")]
        [Description("每次加载更多图片时所需要图片最低数量(多了的话可能会被ban?)")]
        [Range(1,100)]
        public int GetPictureCountPerLoad { get; set; } = 20;

        [Group("View Options")]
        [Description("是否使用滚轮或者拖曳滚动图片列表")]
        [NeedRestart]
        public bool GalleryListScrollBarVisiable { get; set; } = true;

        [Group("View Options")]
        [Description("强制使用图片页面跳转功能，但这可能会花费很长时间和出现大量的网络请求")]
        public bool ForceEnablePageJumpFeature { get; set; } = false;

        [Group("View Options")]
        [NeedRestart]
        [Description("设置画廊浏览列表每个缩略图的宽度(px)")]
        public uint PictureGridItemWidth { get; set; } = 150;

        [Group("View Options")]
        [NeedRestart]
        [Description("设置画廊浏览列表每个缩略图之间的间距(px)")]
        public uint PictureGridItemMarginWidth { get; set; } = 10;

        #endregion

        #region Cache Options

        [Group("Cache Option")]
        [Description("是否使用内存缓存机制，这将会缓存图片文件等其他资源")]
        public bool EnableMemoryCache { get; set; } = false;

        [Group("Cache Option")]
        [Description("是否使用文件缓存机制，这将会缓存图片文件等其他资源")]
        public bool EnableFileCache { get; set; } = false;

        [Group("Cache Option")]
        [Description("钦定缓存文件夹的大小,以MB为单位，若空间不足则会删除较旧的资源缓存文件")]
        [EnableBy(nameof(EnableFileCache), true)]
        public uint CacheFolderMaxSize { get; set; } = 100;

        [Group("Cache Option")]
        [Description("钦定缓存文件夹的存放位置,%Temp%表示系统临时文件夹路径")]
        [EnableBy(nameof(EnableFileCache), true)]
        public string CacheFolderPath { get; set; } = @"%Temp%WbooruCache";

        #endregion

        #region Other Options

        [Description("是否自动检查程序是否有更新，通常会在程序启动时自动检查")]
        [NeedRestart]
        public bool EnableAutoCheckProgramUpdatable { get; set; } = true;

        public enum UpdatableTarget
        {
            Stable, Preview
        }

        [List(typeof(UpdatableTarget), true)]
        [NeedRestart]
        public UpdatableTarget UpdatableTargetVersion { get; set; } = UpdatableTarget.Stable;

        [Path(false, false)]
        [NeedRestart]
        public string LogOutputDirectory { get; set; } = "./Logs";

        [NeedRestart]
        public bool EnableOutputDebugMessage { get; set; } = false;

        public enum LogWindowShowOption
        {
            None,
            OnlyDebugEnable,
            Always
        }

        [NeedRestart]
        [List(typeof(LogWindowShowOption), true)]
        [Description("是否显示一个控制台窗口来显示日志内容")]
        public LogWindowShowOption ShowOutputWindow { get; set; } = LogWindowShowOption.OnlyDebugEnable;

        [Description("是否记住当前窗口的大小和位置，下次打开会按照上次布局来恢复大小和位置")]
        public bool RememberWindowSizeAndLocation { get; set; } = true;

        #endregion

        #region Network Options

        [Description("是否开启Socks5代理功能")]
        [Group("Network Option")]
        [NeedRestart]
        public bool EnableSocks5Proxy { get; set; } = false;

        [Description("Socks5代理地址")]
        [Group("Network Option")]
        [EnableBy(nameof(EnableSocks5Proxy),true)]
        public string Socks5ProxyAddress { get; set; } = "localhost";

        [Description("Socks5代理地址的端口号")]
        [Group("Network Option")]
        [EnableBy(nameof(EnableSocks5Proxy), true)]
        public int Socks5ProxyPort { get; set; } = 1080;

        #endregion

        #region Deep Dark Fantasty

        #region Special Options

        [Group("Advanced Option")]
        [Description("是否显示高级选项(不建议修改其选项值)")]
        public bool ShowAdvancedOptions { get; set; } = false;

        /*
        [Group("Advanced Option")]
        [EnableBy(nameof(ShowAdvancedOptions),true)]
        [Description("程序的Github地址，通常用于更新功能")]
        public string ProgramRepoURL { get; set; } = "https://github.com/MikiraSora/Wbooru";
        */

        [Group("Advanced Option")]
        [EnableBy(nameof(ShowAdvancedOptions), true)]
        [Description("Sqlite数据库文件的路径")]
        public string DBFilePath { get; set; } = "data.db";

        #endregion

        [Ignore]
        public bool IgnoreSettingChangedComfirm { get; set; } = false;

        /*
         * 用于记录窗口的，下次开启会恢复成上次的大小和位置
         * 其中一个为0的话则不恢复
         */
        [Ignore]
        public Size? WindowSize { get; set; } = null;
        [Ignore]
        public Point? WindowLocation { get; set; } = null;

        #endregion
    }
}
