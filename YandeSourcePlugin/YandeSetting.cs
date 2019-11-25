using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;

namespace YandeSourcePlugin
{
    [Export(typeof(SettingBase))]
    public class YandeSetting:SettingBase
    {
        [NeedRestart]
        [Group("View Options")]
        [Description("每次加载图片信息的数量,0代表采用软件GlobalSetting的GetPictureCountPerLoad选项")]
        [Range(0,100)]
        public int PicturesCountPerRequest { get; set; } = 20;

        [Group("Vote Options")]
        [Description("投票默认值")]
        [Range(1, 3)]
        public int VoteValue { get; set; } = 1;
    }
}
