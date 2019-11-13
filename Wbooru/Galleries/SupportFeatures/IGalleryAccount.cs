using Wbooru.UI.Pages;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryAccount : IGalleryFeature
    {
        /*
         * 通常CustomizedLoginPage表示是否自己实现一个自定义的登陆界面，以及相关的登陆逻辑实现，那么在这个页面插件就得自己自主调用AccountLogin()
         * 再通过NavigationHelper来退出此自定义页面，结束用户登陆过程
         * AccountLogout()则由Wbooru来调用
         */

        bool IsLoggined { get; }
        CustomLoginPage CustomLoginPage { get; }

        void AccountLogin(AccountInfo info);
        void AccountLogout();
    }
}
