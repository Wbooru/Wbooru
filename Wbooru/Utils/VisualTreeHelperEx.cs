﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Wbooru.Utils
{
    public static class VisualTreeHelperEx
    {
        public static FrameworkElement FindName(string name, FrameworkElement root) => Find(x => x.Name == name, root);

        public static IEnumerable<T> GetAllRecursively<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj is T t)
                yield return t;
            foreach (var i in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(obj)).Select(x => VisualTreeHelper.GetChild(obj, x)).OfType<DependencyObject>().Select(x => GetAllRecursively<T>(x)).SelectMany(x => x))
                yield return i;
        }

        public static FrameworkElement Find(Func<FrameworkElement, bool> check, FrameworkElement root)
        {
            Stack<FrameworkElement> tree = ObjectPool<Stack<FrameworkElement>>.Get();
            tree.Clear();

            tree.Push(root);

            while (tree.Count > 0)
            {
                FrameworkElement current = tree.Pop();
                if (check(current))
                {
                    ObjectPool<Stack<FrameworkElement>>.Return(tree);
                    return current;
                }

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is FrameworkElement)
                        tree.Push((FrameworkElement)child);
                }
            }

            ObjectPool<Stack<FrameworkElement>>.Return(tree);
            return null;
        }
    }
}
