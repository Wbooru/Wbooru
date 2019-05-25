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
    [Export(typeof(NavigationHelper))]
    public class NavigationHelper
    {
        Stack<Page> page_stack=new Stack<Page>();
        private readonly NavigationService service;

        [ImportingConstructor]
        public NavigationHelper([Import(typeof(Frame))]Frame main_frame)
        {
            service = main_frame.NavigationService;
        }

        public void NavigationPush(Page page)
        {
            Log<NavigationHelper>.Debug($"Push page : {page.ToString()}");
            page_stack.Push(page);
            service.Navigate(page_stack.Peek());
        }

        public Page NavigationPop()
        {
            var page = page_stack.Pop();
            Log<NavigationHelper>.Debug($"Pop page : {page.ToString()}");

            service.Navigate(page_stack.Peek());
            Log<NavigationHelper>.Debug($"Current page : {page_stack.Peek().ToString()}");

            return page;
        }
    }
}
