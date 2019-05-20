using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wbooru.UI.Panels
{
    class ItemLinearContainerCalcStack
    {
        public List<UIElement> Children { get; } = new List<UIElement>();

        Size desired_size = new Size(0, 0);
        public Size DesiredSize => desired_size;

        public void Add(UIElement element)
        {
            Children.Add(element);

            //update DisiredSize
            desired_size.Width = Math.Max(desired_size.Width, element.DesiredSize.Width);
            desired_size.Height += element.DesiredSize.Height;
        }
    }

    public class GridFlowPanel : Panel
    {
        #region DP

        public int GridItemWidth
        {
            get { return (int)GetValue(GridItemWidthProperty); }
            set { SetValue(GridItemWidthProperty, value); }
        }

        public int GridItemMarginWidth
        {
            get { return (int)GetValue(GridItemMarginWidthProperty); }
            set { SetValue(GridItemMarginWidthProperty, value); }
        }

        public static readonly DependencyProperty GridItemMarginWidthProperty =
            DependencyProperty.Register("GridItemMarginWidth", typeof(int), typeof(GridFlowPanel), new PropertyMetadata(10));

        public static readonly DependencyProperty GridItemWidthProperty =
            DependencyProperty.Register("GridItemWidth", typeof(int), typeof(GridFlowPanel), new PropertyMetadata(150));

        #endregion

        ItemLinearContainerCalcStack[] containers = default;

        protected override Size MeasureOverride(Size availableSize)
        {
            Console.WriteLine(availableSize);

            Size size = default;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(availableSize.Width, availableSize.Height));

                if (containers == null)
                {
                    int col = (int)((double.IsNaN(Width) ? 0 : Width) / child.DesiredSize.Width);

                    containers = new ItemLinearContainerCalcStack[col];

                    for (int i = 0; i < containers.Length; i++)
                        containers[i] = new ItemLinearContainerCalcStack();
                }

                var container = containers.OrderBy(x => x.DesiredSize.Height).FirstOrDefault();

                container?.Add(child);
            }

            var width = (containers?.Length ?? 0) != 0 ? containers.Max(x => x.DesiredSize.Width) : 0;
            var height = (containers?.Length ?? 0) != 0 ? containers.Max(x => x.DesiredSize.Height) : 0;

            size.Width = double.IsPositiveInfinity(availableSize.Width) ? width : availableSize.Width;
            size.Height = double.IsPositiveInfinity(availableSize.Height) ? height : availableSize.Height;

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var current_x = 0d;

            //calculate X offset so that we could make children element in center.
            if (!double.IsPositiveInfinity(finalSize.Width) && (containers?.Any() ?? false))
            {
                var offset_x = (finalSize.Width - containers.Aggregate(0d, (t, x) => x.DesiredSize.Width + t)) / 2;

                if (offset_x <= GridItemWidth)
                    current_x += offset_x;
            }

            for (int i = 0; i < (containers?.Length ?? 0); i++)
            {
                var container = containers[i];

                var current_y = 0d;

                foreach (var child in container.Children)
                {
                    child.Arrange(new Rect(current_x, current_y, child.DesiredSize.Width, child.DesiredSize.Height));

                    current_y += child.DesiredSize.Height;
                }

                current_x += container.DesiredSize.Width;
            }

            containers = null;

            return finalSize;
        }
    }
}
