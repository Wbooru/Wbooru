using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Wbooru.Kernel;
using Wbooru.Utils;

namespace Wbooru.UI.Dialogs
{
    public static class Dialog
    {
        private static Dictionary<DialogContentHost, Task> await_tokens = new Dictionary<DialogContentHost, Task>();

        private static Grid DialogLayer { get; set; }
        private static FrameworkElement BackgroundElement { get; set; }

        private static Storyboard begin_blur_sb, end_blur_sb;

        internal static void Init(Grid layer, FrameworkElement background)
        {
            DialogLayer = layer;
            BackgroundElement = background;

            //Add blur effect and wait for storyboard.
            BlurEffect blur_effect = new BlurEffect()
            {
                Radius = 0
            };

            begin_blur_sb = new Storyboard();

            var animation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(250),
                To = 7,
                From = 0,
            };

            begin_blur_sb.Children.Add(animation);

            Storyboard.SetTargetProperty(animation, new PropertyPath(BlurEffect.RadiusProperty));
            Storyboard.SetTargetName(animation, nameof(blur_effect));


            end_blur_sb = new Storyboard();


            var animation2 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(250),
                To = 0,
                From = 7,
            };

            end_blur_sb.Children.Add(animation2);

            Storyboard.SetTargetProperty(animation2, new PropertyPath(BlurEffect.RadiusProperty));
            Storyboard.SetTargetName(animation2, nameof(blur_effect));


            if (background.Effect != null)
                ExceptionHelper.DebugThrow(new Exception("background exists effect"));

            background.Effect = blur_effect;
            background.RegisterName(nameof(blur_effect), blur_effect);
        }

        private static void BeginDialogEffect()
        {
            Log.Debug($"add blur effect");
            begin_blur_sb.Begin(BackgroundElement);
        }

        private static void EndDialogEffect()
        {
            Log.Debug($"remove blur effect");
            end_blur_sb.Begin(BackgroundElement);
        }

        public static Task ShowDialog<T>() where T : FrameworkElement, new()
        {
            var panel = new T();

            Log.Debug($"Start new dialog with content : {panel.Name}({panel.GetType().Name})");

            var dialog = new DialogContentHost();
            dialog.Content = panel;

            var task = new Task(() => { });

            await_tokens[dialog] = task;

            DialogLayer.Children.Add(dialog);

            if (DialogLayer.Children.Count == 1)
                BeginDialogEffect();

            return task;
        }

        public static void CloseDialog<T>(T dialog) where T : DialogContentHost
        {
            Log.Debug($"Try close dialog : {dialog.Name}");

            var task = await_tokens[dialog];

            DialogLayer.Children.Remove(dialog);

            task.Start();

            if (DialogLayer.Children.Count == 0)
                EndDialogEffect();
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
