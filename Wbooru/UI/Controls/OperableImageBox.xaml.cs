using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// OperableImageBox.xaml 的交互逻辑
    /// </summary>

    public partial class OperableImageBox : UserControl
    {
        private enum DragActionState
        {
            Idle,
            ReadyDrag,
            Dragging
        }

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(OperableImageBox), new PropertyMetadata(null, (d, e) => 
            (d as OperableImageBox).ApplyImage(e.NewValue as BitmapSource)));

        public enum ScaleState
        {
            NoScale,
            Scale2x,
            Scale4x,
            ScaleRawPixel
        }

        private DragActionState drag_action_state = DragActionState.Idle;
        private ScaleState CurrentScale { get; set; }
        private Vector CurrentTranslateOffset { get; set; }
        private Dictionary<ScaleState, Storyboard> TransformScaleMap { get; set; }
        private bool need_raw_pixel;

        private double ImageBoxTranslateTransformX
        {
            get => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X;
            set => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X = value;
        }

        private double ImageBoxTranslateTransformY
        {
            get => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).Y;
            set => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).Y = value;
        }

        private double ImageBoxScaleTransformX
        {
            get => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleX;
            set => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleX = value;
        }

        private double ImageBoxScaleTransformY
        {
            get => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleY;
            set => ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleY = value;
        }

        public OperableImageBox()
        {
            InitializeComponent();

            TransformScaleMap = new Dictionary<ScaleState, Storyboard>() {
                {ScaleState.NoScale,Resources["NoScaleAction"] as Storyboard },
                {ScaleState.Scale2x,Resources["Scale2xAction"] as Storyboard },
                {ScaleState.Scale4x,Resources["Scale4xAction"] as Storyboard },
                {ScaleState.ScaleRawPixel,Resources["ScaleRawPixelAction"]as Storyboard },
            };

            if (ImageCoreBox.Source != null)
            {
                ApplyImage(ImageCoreBox.Source);
            }
        }

        public void ResetTransform()
        {
            ApplyScale(ScaleState.NoScale, null);
            ApplyTranslate(new Vector(0, 0));
        }

        public void ApplyImage(ImageSource source)
        {
            ResetTransform();
            ImageCoreBox.Source = source;

            var pic_width = (source as BitmapSource).PixelHeight;
            var box_width = ImageCoreBox.ActualWidth;

            need_raw_pixel = pic_width > box_width;

            //update ScaleRawPixelAction sb values
            /*
            foreach (var animation in TransformScaleMap[ScaleState.ScaleRawPixel].Children.OfType<DoubleAnimation>())
            {
                animation.To = pic_width * 1.0d / (box_width == 0 ? pic_width : box_width);
            }  
            */
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var point = e.GetPosition(ImageCoreBox);
                point.X = Math.Min(ImageCoreBox.ActualWidth, Math.Max(0, point.X));
                point.Y = Math.Min(ImageCoreBox.ActualHeight, Math.Max(0, point.Y));

                var next_scale = (ScaleState)((((int)CurrentScale) + 1) % 4);

                ApplyScale(next_scale, point);
            }
        }

        public void ApplyScale(ScaleState scale, Point? scale_center_point)
        {
            CurrentScale = scale;
            var point = scale_center_point ?? new Point(ImageCoreBox.ActualWidth / 2, ImageCoreBox.ActualHeight / 2);

            ((ImageCoreBox.RenderTransform as TransformGroup).Children[0] as ScaleTransform).CenterX = point.X;
            ((ImageCoreBox.RenderTransform as TransformGroup).Children[0] as ScaleTransform).CenterY = point.Y;

            var sb = TransformScaleMap[CurrentScale];

            sb.Begin(ImageCoreBox);

            ReboundImageBox();
        }

        public void ApplyTranslate(Vector offset)
        {
            /*
            ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X = offset.X;
            ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).Y = offset.Y;

            CurrentTranslateOffset = new Vector(((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X, ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).Y);
            Console.WriteLine(CurrentTranslateOffset);
            */
        }

        private void ReboundImageBox()
        {
            /*
                p11  ----------  p12
                 |                |
                 |                |
                 |                |
                p21  ----------  p22
             */
            Vector offset = CurrentTranslateOffset;

            var p11 = ImageCoreBox.TranslatePoint(new Point(0, 0), WrapPanel);
            var p12 = new Point(p11.X + ImageCoreBox.ActualWidth, p11.Y);
            var p21 = new Point(p11.X, p11.Y + ImageCoreBox.ActualHeight);
            var p22 = new Point(p11.X + ImageCoreBox.ActualWidth, p11.Y + ImageCoreBox.ActualHeight);

            if (p11.Y > 0)
            {
                offset.Y = CurrentTranslateOffset.Y - p11.Y;
                Console.WriteLine(">0");
            }
            else if (p21.Y < WrapPanel.ActualHeight)
            {
                offset.Y = CurrentTranslateOffset.Y + (-p11.Y);
                Console.WriteLine($"<{WrapPanel.ActualHeight}");
            }

            ApplyTranslate(offset);
        }

        Point prev_point;

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag_action_state == DragActionState.ReadyDrag)
            {
                drag_action_state = DragActionState.Dragging;

                prev_point = e.GetPosition(this);
                Console.WriteLine($"Grid_MouseMove:{drag_action_state} {prev_point}");
            }
            else if (drag_action_state == DragActionState.Dragging)
            {
                var now_point = e.GetPosition(this);
                var offset = now_point - prev_point;

                ApplyTranslate(CurrentTranslateOffset + offset);
                ReboundImageBox();
                Console.WriteLine($"Grid_MouseMove:{drag_action_state} {prev_point}");
                prev_point = now_point;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            drag_action_state = DragActionState.ReadyDrag;
            Console.WriteLine($"Grid_MouseLeftButtonDown:{drag_action_state}");
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            drag_action_state = DragActionState.Idle;
            Console.WriteLine($"Grid_MouseLeftButtonUp:{drag_action_state}");
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            drag_action_state = DragActionState.Idle;
            Console.WriteLine($"Grid_MouseLeave:{drag_action_state}");
        }
    }
}
