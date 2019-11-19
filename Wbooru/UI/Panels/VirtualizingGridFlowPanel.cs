using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Wbooru.UI.Panels
{
    public class VirtualizingGridFlowPanel : VirtualizingPanel
    {
        public Dictionary<int, Rect> calculated_item_rects = new Dictionary<int, Rect>();

        public VirtualizingGridFlowPanel()
        {

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var generator = ItemContainerGenerator as ItemContainerGenerator;
                var index = generator?.IndexFromContainer(child) ?? InternalChildren.IndexOf(child);

                if (!calculated_item_rects.TryGetValue(index,out var layout))
                    continue;

                //layout.Offset(_offset.X * -1, _offset.Y * -1);
                child.Arrange(layout);
            }

            return finalSize;
        }
    }
}
