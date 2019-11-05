using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Wbooru.Kernel
{
    public static class NavigationHelper
    {
        static Stack<Page> page_stack=new Stack<Page>();
        private static NavigationService service;

        public static void InitNavigationHelper(Frame main_frame)
        {
            service = main_frame.NavigationService;
        }

        public static void NavigationPush(Page page)
        {
            Log.Debug($"Push page : {page.ToString()}");
            page_stack.Push(page);
            service.Navigate(page_stack.Peek());
        }

        public static Page NavigationPop()
        {
            var page = page_stack.Pop();
            Log.Debug($"Pop page : {page.ToString()}");

            service.Navigate(page_stack.Peek());
            Log.Debug($"Current page : {page_stack.Peek().ToString()}");

            return page;
        }
    }
}
