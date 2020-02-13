using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using Wbooru.Utils;

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

            ClearJournalEnties();

            service.Navigate(page_stack.Peek());
        }

        private static void ClearJournalEnties()
        {
            try
            {
                const BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
                var journal_scope_type = service.GetType().GetProperty("JournalScope", flag);
                var journal_scope = journal_scope_type.GetValue(service);

                if (journal_scope != null)
                {
                    var journal_type = journal_scope_type.PropertyType.GetProperty("Journal", flag);
                    var journal = journal_type.GetValue(journal_scope);

                    var remove_method = journal.GetType().GetMethod("RemoveEntryInternal", flag);

                    var journal_entries_count = (journal.GetType().GetProperty("BackStack", flag).GetValue(journal) as IEnumerable)
                        .OfType<object>()
                        .Count();

                    for (int i = 0; i < journal_entries_count; i++)
                        remove_method.Invoke(journal, new object[] { 0 });

                    if (journal_entries_count != 0)
                        Log.Debug($"Removed {journal_entries_count} journal entries");
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                Log.Error("Can't not clean journal entries cache");
            }
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

            ClearJournalEnties();

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
