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
        [NameAlias("开启标签过滤功能")]
        public bool EnableTagFilter { get; set; } = false;

        [Group("Tag Filter Options")]
        [List(typeof(TagFilterTarget), true)]
        [EnableBy(nameof(EnableTagFilter),true)]
        [NameAlias("需要标签过滤的目标")]
        public TagFilterTarget FilterTarget { get; set; } = TagFilterTarget.MainWindow | TagFilterTarget.SearchResultWindow;

        [Group("Tag Filter Options")]
        [EnableBy(nameof(EnableTagFilter), true)]
        [NameAlias("过滤列表是否共用")]
        public bool UseAllGalleryFilterList { get; set; } = false;

        #endregion

        #region Tags Options

        [Group("Tags Options")]
        [Description("在查看图片详细内容时尝试获取标签数据(染色)，此操作将会占用大量空间和网络带宽")]
        [NameAlias("获取并缓存标签数据")]
        public bool PredownloadAndCacheTagData { get; set; } = false;

        [Group("Tags Options")]
        [NameAlias("严格搜索标签元数据")]
        public bool SearchTagMetaStrict { get; set; } = false;

        [Group("Tags Options")]
        [Description("搜索图片标签时，标签建议最大数量,0代表不限制")]
        [Range(0,1000)]
        [NameAlias("标签搜索候选列表最大呈现数量")]
        public int MaxSearchSuggestsCount { get; set; } = 100;

        #endregion

        #region Download Options

        [Group("Download Options")]
        [Path(false,false)]
        [NameAlias("图片下载保存路径")]
        public string DownloadPath { get; set; } = "./Downloads";

        [Group("Download Options")]
        [Description("将会按照\"...Downloads\\Yande\\xxx.png\"形式存放")]
        [NameAlias("将下载的图片分别按照画廊名字文件夹存放")]
        public bool SeparateGallerySubDirectories { get; set; } = true;

        #endregion

        #region View Options

        [Group("View Options")]
        [Description("若开启将会过滤不合适公开浏览的图片,以及不支持NSFW过滤实现的图源,但本地收藏列表和下载列表不受限制.")]
        [NeedRestart]
        [NameAlias("开启NSFW模式")]
        public bool EnableNSFWFileterMode { get; set; } = false;

        [Group("View Options")]
        [Range(1,5)]
        [NameAlias("图片加载线程数")]
        public int LoadingImageThread { get; set; } = 2;

        [Group("View Options")]
        [Description("每次加载更多图片时所需要图片最低数量(多了的话可能会被ban?)")]
        [Range(1,100)]
        [NameAlias("每批图片加载数量")]
        public int GetPictureCountPerLoad { get; set; } = 20;

        [Group("View Options")]
        [Description("如果开启则显示右边页面滑条,但不能滑动页面")]
        [NeedRestart]
        [NameAlias("画廊列表显示滑条")]
        public bool GalleryListScrollBarVisiable { get; set; } = true;

        [Group("View Options")]
        [Description("这可能会花费很长时间和出现大量的网络请求")]
        [NameAlias("强制使用图片页面跳转功能")]
        public bool ForceEnablePageJumpFeature { get; set; } = false;

        [Group("View Options")]
        [NeedRestart]
        [Description("单位是像素(px)")]
        [NameAlias("画廊浏览列表每个缩略图的宽度")]
        public uint PictureGridItemWidth { get; set; } = 250;

        [Group("View Options")]
        [NeedRestart]
        [Description("单位是像素(px)")]
        [NameAlias("画廊浏览列表每个缩略图之间的间距")]
        public uint PictureGridItemMarginWidth { get; set; } = 10;

        public enum SelectViewQualityTarget
        {
            Lowest,
            Lower,
            Middle,
            Higher,
            Highest
        }

        [Group("View Options")]
        [List(typeof(SelectViewQualityTarget), true)]
        [Description("即会按照文件大小排序并根据值来选择合适的图片来加载，越低文件越小，画质可能越差")]
        [NameAlias("详细页面图片质量")]
        public SelectViewQualityTarget SelectPreferViewQualityTarget { get; set; } = SelectViewQualityTarget.Middle;

        [Group("Other Options")]
        [Description("解决一些图片\"unknown file size\"问题，但开启此项可能会有大量的网络请求")]
        [NameAlias("尝试获取一些可下载图片的文件大小值")]
        public bool TryGetVaildDownloadFileSize { get; set; } = false;
        #endregion

        #region Cache Options

        [Group("Cache Option")]
        [Description("将一些缓存资源存放在指定的缓存文件夹中，用户可以随便删除(但不建议修改其中文件)")]
        [NameAlias("启用文件缓存")]
        public bool EnableFileCache { get; set; } = true;

        [Group("Cache Option")]
        [NeedRestart]
        [Description("%Temp%表示系统临时文件夹路径")]
        [EnableBy(nameof(EnableFileCache), true)]
        [NameAlias("缓存文件夹的存放路径")]
        public string CacheFolderPath { get; set; } = @"%Temp%WbooruCache";

        #endregion

        #region Other Options

        [Description("通常会在程序启动时自动开始检查")]
        [NeedRestart]
        [NameAlias("自动检查程序和插件是否有更新")]
        public bool EnableAutoCheckUpdatable { get; set; } = true;

        public enum UpdatableTarget
        {
            Stable, Preview
        }

        [List(typeof(UpdatableTarget), true)]
        [NeedRestart]
        [Description("Stable为稳定正式版，Preview为预览版")]
        [NameAlias("可更新版本类型")]
        public UpdatableTarget UpdatableTargetVersion { get; set; } = UpdatableTarget.Stable;

        [Path(false, false)]
        [NeedRestart]
        [NameAlias("日志输出文件夹路径")]
        public string LogOutputDirectory { get; set; } = "./Logs";

        [NeedRestart]
        [NameAlias("输出调试类日志信息")]
        public bool EnableOutputDebugMessage { get; set; } = false;

        public enum LogWindowShowOption
        {
            None,
            OnlyDebugEnable,
            Always
        }

        [NeedRestart]
        [List(typeof(LogWindowShowOption), true)]
        [NameAlias("显示控制台窗口来显示日志内容")]
        public LogWindowShowOption ShowOutputWindow { get; set; } = LogWindowShowOption.OnlyDebugEnable;

        [Description("下次打开会按照上次布局来恢复大小和位置")]
        [NameAlias("记住当前窗口的大小和位置")]
        public bool RememberWindowSizeAndLocation { get; set; } = true;

        public enum TagListOrder
        {
            Name,
            AddedDateTime
        }

        [Description("主页面的标签浏览控件里，标签列表的排序方式")]
        [List(typeof(TagListOrder))]
        [NeedRestart]
        [NameAlias("初始标签列表排列顺序")]
        public TagListOrder TagListViewerListOrder { get; set; } = TagListOrder.AddedDateTime;

        #endregion

        #region Network Options

        [Group("Network Option")]
        [Description("0为不限制，单位为毫秒ms")]
        [NameAlias("网络请求的超时限")]
        public int RequestTimeout { get; set; } = 15000;

        [NameAlias("开启Socks5代理功能")]
        [Group("Network Option")]
        [NeedRestart]
        [Description("不懂的好孩子可以不用管~")]
        public bool EnableSocks5Proxy { get; set; } = false;

        [NameAlias("Socks5代理地址")]
        [Group("Network Option")]
        [EnableBy(nameof(EnableSocks5Proxy),true)]
        public string Socks5ProxyAddress { get; set; } = "localhost";

        [NameAlias("Socks5代理地址的端口号")]
        [Group("Network Option")]
        [EnableBy(nameof(EnableSocks5Proxy), true)]
        public int Socks5ProxyPort { get; set; } = 1080;

        #endregion

        #region Deep Dark Fantasty

        #region Special Options

        [Group("Advanced Option")]
        [NameAlias("显示高级选项(不建议修改其选项值)")]
        public bool ShowAdvancedOptions { get; set; } = false;

        /*
        [Group("Advanced Option")]
        [EnableBy(nameof(ShowAdvancedOptions),true)]
        [Description("程序的Github地址，通常用于更新功能")]
        public string ProgramRepoURL { get; set; } = "https://github.com/MikiraSora/Wbooru";
        */

        [Group("Advanced Option")]
        [EnableBy(nameof(ShowAdvancedOptions), true)]
        [NameAlias("Sqlite数据库文件的路径")]
        [NeedRestart]
        public string DBFilePath { get; set; } = "data.db";

        [Group("Advanced Option")]
        [EnableBy(nameof(ShowAdvancedOptions), true)]
        [NeedRestart]
        [NameAlias("输出数据库日志到调试日志里")]
        public bool EnableDatabaseLog { get; set; } = false;

        #endregion

        /// <summary>
        /// 钦定本次运行中需要重启的设置变更了，是否不提醒用户重启。
        /// </summary>
        internal bool IgnoreSettingChangedComfirm { get; set; } = false;

        /*
         * 用于记录窗口的，下次开启会恢复成上次的大小和位置
         * 其中一个为0的话则不恢复
         */
        [Ignore]
        public Size? WindowSize { get; set; } = null;
        [Ignore]
        public Point? WindowLocation { get; set; } = null;

        [Ignore]
        public string RememberLastViewedGalleryName { get; set; } = string.Empty;

        #endregion
    }
}
