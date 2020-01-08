using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wbooru.Kernel;
using Wbooru.Utils;

namespace Wbooru.UI.Dialogs
{
    public static class Dialog
    {
        private static Dictionary<DialogContentHost, Task> await_tokens = new Dictionary<DialogContentHost, Task>();

        private static Grid DialogLayer { get; set; }

        internal static void Init(Grid layer) => DialogLayer = layer;

        public static Task ShowDialog<T>() where T : FrameworkElement, new()
        {
            var panel = new T();

            Log.Debug($"Start new dialog with content : {panel.Name}({panel.GetType().Name})");

            var dialog = new DialogContentHost();
            dialog.Content = panel;

            var task = new Task(() => { });

            await_tokens[dialog] = task;

            DialogLayer.Children.Add(dialog);

            return task;
        }

        public static void CloseDialog<T>(T dialog) where T : DialogContentHost
        {
            Log.Debug($"Try close dialog : {dialog.Name}");

            var task = await_tokens[dialog];

            DialogLayer.Children.Remove(dialog);

            task.Start();
        }

        public static void CloseDialog(FrameworkElement content)
        {
            Log.Debug($"Try close dialog via framework element content : {content.Name}({content.GetType().Name})");

            var dialog = await_tokens.Keys.FirstOrDefault(x=>x.Content == content);

            if (dialog is null)
                ExceptionHelper.DebugThrow(new Exception("Can't reference host dialog from content param."));

            CloseDialog(dialog);
        }
    }
}
