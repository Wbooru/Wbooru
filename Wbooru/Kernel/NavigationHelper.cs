using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
            service.Navigated += Service_Navigated;
        }

        private static void Service_Navigated(object sender, NavigationEventArgs e)
        {
            Log.Debug($"Navigated : ({e?.Content?.GetHashCode()}){e.Uri}");
            //service.RemoveBackEntry();
        }

        public static void NavigationPush(Page page)
        {
            Log.Debug($"Push page : {page.ToString()}");
            page_stack.Push(page);
            service.Navigate(page_stack.Peek());
        }

        public static Page NavigationPop()
        {
            if (page_stack.Count == 1)
            {
                Log.Warn($"page_stack will be empty so it not allow pop current page.");
                return null;
            }

            var page = page_stack.Pop();
            Log.Debug($"Pop page : {page.ToString()}");

            service.Navigate(page_stack.Peek());
            Log.Debug($"Current page : {page_stack.Peek().ToString()}");

            return page;
        }

        internal static void RequestPageBackAction()
        {
            if (page_stack.Count == 0)
                return;

            if (page_stack.Peek() is INavigatableAction action)
                action.OnNavigationBackAction();
            else
                NavigationPop();
        }

        internal static void RequestPageForwardAction()
        {
            if (page_stack.Count == 0)
                return;

            (page_stack.Peek() as INavigatableAction)?.OnNavigationForwardAction();
        }
    }
}
