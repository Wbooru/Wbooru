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
            NoScale = 1,
            Scale2x = 2,
            Scale4x = 4,
            ScaleRawPixel = 0
        }

        private DragActionState drag_action_state = DragActionState.Idle;
        private ScaleState CurrentScale { get; set; }
        private Vector CurrentTranslateOffset { get; set; }
        private Dictionary<ScaleState, Storyboard> TransformScaleMap { get; set; }

        private readonly static ScaleState[] SWITCH_ROLL = { ScaleState.NoScale, ScaleState.Scale2x, ScaleState.Scale4x, ScaleState.ScaleRawPixel };
        private int current_switch_index = 0;

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

            foreach (var sb in TransformScaleMap.Values)
                sb.Completed += (_, __) => ReboundImageBox();

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

            if (source==null)
                return;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var point = e.GetPosition(ImageCoreBox);
                point.X = Math.Min(ImageCoreBox.ActualWidth, Math.Max(0, point.X));
                point.Y = Math.Min(ImageCoreBox.ActualHeight, Math.Max(0, point.Y));

                current_switch_index = (++current_switch_index) % SWITCH_ROLL.Length;
                var next_scale = SWITCH_ROLL[current_switch_index];

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
        }

        public void ApplyTranslate(Vector offset)
        {
            ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X = offset.X;
            ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).Y = offset.Y;

            CurrentTranslateOffset = new Vector(((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).X, ((ImageCoreBox.RenderTransform as TransformGroup).Children[1] as TranslateTransform).Y);
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

            var scaled_size = GetScaledPictureSize();

            var p11 = ImageCoreBox.TranslatePoint(new Point(0, 0), WrapPanel);
            var p22 = new Point(p11.X + scaled_size.Width, p11.Y + scaled_size.Height);

            if (scaled_size.Height >= WrapPanel.ActualHeight)
            {
                if (p11.Y > 0)
                {
                    offset.Y = CurrentTranslateOffset.Y - p11.Y;
                    Console.WriteLine($">0 p11 = {p11}  |  p22 = {p22} | CTO = {CurrentTranslateOffset}");
                }
                else if (p22.Y < WrapPanel.ActualHeight)
                {
                    offset.Y = CurrentTranslateOffset.Y + (WrapPanel.ActualHeight - p22.Y);
                    Console.WriteLine($"<{WrapPanel.ActualHeight}  p11 = {p11}  |  p22 = {p22} | CTO = {CurrentTranslateOffset}");
                }
            }

            if (scaled_size.Width >= WrapPanel.ActualWidth)
            {
                if (p11.X > 0)
                {
                    offset.X = CurrentTranslateOffset.X - p11.X;
                    Console.WriteLine($">0 p11 = {p11}  |  p22 = {p22} | CTO = {CurrentTranslateOffset}");
                }
                else if (p22.X < WrapPanel.ActualWidth)
                {
                    offset.X = CurrentTranslateOffset.X + (WrapPanel.ActualWidth - p22.X);
                    Console.WriteLine($"<{WrapPanel.ActualHeight}  p11 = {p11}  |  p22 = {p22} | CTO = {CurrentTranslateOffset}");
                }
            }

            ApplyTranslate(offset);
        }

        private Size GetScaledPictureSize()
        {
            var pic = ImageCoreBox.Source as BitmapSource;
            var width = ImageCoreBox.ActualWidth;
            var height = ImageCoreBox.ActualHeight;

            var scale = (int)CurrentScale;

            if (scale!=0)
            {
                width *= scale;
                height *= scale;
            }
            else
            {
                width = pic.PixelWidth;
                height = pic.PixelHeight;
            }

            return new Size(width, height);
        }

        Point prev_point;

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (drag_action_state == DragActionState.ReadyDrag)
            {
                drag_action_state = DragActionState.Dragging;
                Console.WriteLine($"Grid_MouseMove:{drag_action_state} {prev_point}");
            }
            else if (drag_action_state == DragActionState.Dragging)
            {
                var now_point = e.GetPosition(this);
                var offset = now_point - prev_point;

                ApplyTranslate(CurrentTranslateOffset + offset);
                Console.WriteLine($"Grid_MouseMove:{drag_action_state} {prev_point}");
                prev_point = e.GetPosition(this);
                ReboundImageBox();
            }

            prev_point = e.GetPosition(this);
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
