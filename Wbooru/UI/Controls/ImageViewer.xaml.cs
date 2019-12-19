using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wbooru.Utils;

namespace Wbooru.UI.Controls
{
    /// <summary>
    /// ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        #region delegate / events

        public delegate void ScaleChangedHandler(double ratio);
        public event ScaleChangedHandler ScaleChanged;

        #endregion

        #region Dependency properties

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                typeof(ImageSource),
                typeof(ImageViewer),
                new PropertyMetadata(null, ImageSourceChanged));

        private static void ImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageViewer viewer)
                viewer.LoadImage(e.NewValue as ImageSource);
        }

        #endregion

        #region Public Properties

        public Size SourceSize { get; private set; }
        public double MinScale { get; set; } = 1;
        public double MaxScale { get; set; } = 5;
        public int ScaleCount { get; set; } = 12;
        public TimeSpan AnimationTime { get; set; } = TimeSpan.Zero;

        public string DefaultRatios
        {
            get => _defaultRatios;
            set
            {
                _defaultRatios = value;
                _defaultRatioArray = value?.Split(',')
                                         .Select(double.Parse)
                                         .OrderBy(k => k)
                                         .Distinct().ToArray() ?? Array.Empty<double>();
            }
        }

        public double CurrentScale
        {
            get => _currentScale;
            set
            {
                if (Math.Abs(_currentScale - value) <= 0.01)
                    return;
                ScaleChanged?.Invoke(value);
                SetScale(value, true);
            }
        }

        #endregion

        public ImageViewer()
        {
            InitializeComponent();
        }

        #region Static fields

        private static readonly CircleEase CircleEaseOut = new CircleEase { EasingMode = EasingMode.EaseOut };
        private static readonly Func<double, double> QuadIn = x => x * x;

        #endregion

        #region Readonly fields

        private readonly HashSet<Rectangle> _boundSbList = new HashSet<Rectangle>();

        #endregion

        #region Property fields

        private double _currentScale;
        private string _defaultRatios;
        private double[] _defaultRatioArray = Array.Empty<double>();

        #endregion

        #region Control fields

        //private double _previousScaleIndex = 0;
        private Point _prevImageRel = new Point(double.MinValue, double.MinValue);
        private bool _isMouseDown;
        private double _previewScaleRatio = 0;

        #endregion

        #region Private properties

        private double ImgRatio => SourceSize.Width / SourceSize.Height;
        private double CanvasRatio => ActualWidth / ActualHeight;
        private double FullScaleRatio { get; set; }
        private bool AutoFitLargeSize { get; } = true;

        #endregion

        public void LoadImage(ImageSource image)
        {
            SourceSize = image == null ? new Size() : new Size(image.Width, image.Height);
            FixFullRatio();
            ResetPosition();
            Image.Source = image;
            _boundSbList.Clear();
        }

        public void LoadImage(string path)
        {
            var bitmap = new BitmapImage();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            LoadImage(bitmap);
        }

        private double GetNextDefaultScaleRatio()
        {
            List<double> copy;
            if (_defaultRatioArray[0] == 0)
            {
                copy = _defaultRatioArray.Skip(1).ToList();
                var fullScaleRatio = 1 / FullScaleRatio;
                if (fullScaleRatio < 1)
                    copy.Insert(0, fullScaleRatio);
            }
            else
                copy = _defaultRatioArray.ToList();

            var nextRatio = _currentScale >= copy[copy.Count - 1]
                ? copy[0]
                : copy.First(k => _currentScale < k);
            return nextRatio;
        }

        private double GetNextScaleRatio(bool add)
        {
            var prevScaleIndex = GetRoundedScaleIndexByScaleRatio(add);
            var scaleIndex = add ? prevScaleIndex + 1 : prevScaleIndex - 1;
            //Console.WriteLine(scaleIndex);

            var scaleRatio = GetScaleRatioIndex(scaleIndex);
            return scaleRatio;
        }

        private int GetRoundedScaleIndexByScaleRatio(bool floor)
        {
            var trueMin = Math.Min(1 / FullScaleRatio, MinScale);
            var dblIndex = Math.Round(Math.Pow((_currentScale - trueMin) / (MaxScale - trueMin), 0.5) * ScaleCount, 5);
            return (int)(floor ? Math.Floor(dblIndex) : Math.Ceiling(dblIndex));
        }

        private double GetScaleRatioIndex(int scaleIndex)
        {
            if (scaleIndex > ScaleCount) scaleIndex = ScaleCount;
            else if (scaleIndex < 0) scaleIndex = 0;

            var trueMin = Math.Min(1 / FullScaleRatio, MinScale);
            var scaleRatio = trueMin + (MaxScale - trueMin) * MathEx.QuadIn(scaleIndex / (double)ScaleCount);
            Log.Debug(scaleRatio.ToString());
            return scaleRatio;
        }

        private void MainCanvas_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _previewScaleRatio = GetNextScaleRatio(true);
                if (_previewScaleRatio > MaxScale)
                {
                    SetScale(MaxScale, true);
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                _previewScaleRatio = GetNextScaleRatio(false);
                var finalScale = _previewScaleRatio;
                if (FullScaleRatio <= 1 && finalScale < MinScale)
                {
                    ResetPosition(true);
                    e.Handled = true;
                }
                else if (FullScaleRatio > 1 && finalScale < MinScale / FullScaleRatio)
                {
                    ResetPosition(true);
                    e.Handled = true;
                }
            }
        }

        private void MainCanvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Point imageRelBefore = Mouse.GetPosition(HideBorder);
            //Console.WriteLine($"Before: {nameof(imageRelBefore)}: {imageRelBefore}");

            SetScale(_previewScaleRatio, true);

            Point imageRelAfter = Mouse.GetPosition(HideBorder);
            //Console.WriteLine($"After: {nameof(imageRelAfter)}: {imageRelAfter}");
            SetTranslate(TranslateBorder.X + imageRelAfter.X - imageRelBefore.X,
                TranslateBorder.Y + imageRelAfter.Y - imageRelBefore.Y, true);
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Canvas canvas))
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                Point imageRelBefore = Mouse.GetPosition(HideBorder);
                //Console.WriteLine($"Before: {nameof(imageRelBefore)}: {imageRelBefore}");

                var ratio = GetNextDefaultScaleRatio();
                SetScale(ratio, true);

                Point imageRelAfter = Mouse.GetPosition(HideBorder);
                //Console.WriteLine($"After: {nameof(imageRelAfter)}: {imageRelAfter}");
                SetTranslate(TranslateBorder.X + imageRelAfter.X - imageRelBefore.X,
                    TranslateBorder.Y + imageRelAfter.Y - imageRelBefore.Y, true);
                return;
            }

            _prevImageRel = Mouse.GetPosition(Image);
            canvas.CaptureMouse();
            canvas.Cursor = Cursors.SizeAll;
            _isMouseDown = true;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !(sender is Canvas canvas))
            {
                return;
            }

            var nowImageRel = Mouse.GetPosition(Image);
            if (nowImageRel == _prevImageRel) return;

            //Console.WriteLine(_prevImageRel + ";" + nowImageRel);
            SetTranslate(Translate.X + nowImageRel.X - _prevImageRel.X,
                Translate.Y + nowImageRel.Y - _prevImageRel.Y);
            _prevImageRel = Mouse.GetPosition(Image);
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Canvas canvas))
            {
                return;
            }

            _prevImageRel = new Point(double.MinValue, double.MinValue);
            canvas.ReleaseMouseCapture();
            canvas.Cursor = Cursors.Arrow;
            _isMouseDown = false;
            foreach (var boundRec in _boundSbList)
            {
                ClearBound(boundRec);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FixFullRatio();
            ResetPosition();
        }

        private void ResetPosition(bool animation = false)
        {
            if (AutoFitLargeSize)
            {
                if (FullScaleRatio < 1)
                {
                    SetScale(1, animation);
                }
                else
                {
                    SetScale(1 / FullScaleRatio, animation);
                }
            }
            else
            {
                SetScale(1, animation);
            }

            SetTranslate(0, 0, animation);
        }

        private void FixFullRatio()
        {
            if (ImgRatio >= CanvasRatio)
            {
                FullScaleRatio = SourceSize.Width / ActualWidth;
            }
            else if (ImgRatio < CanvasRatio)
            {
                FullScaleRatio = SourceSize.Height / ActualHeight;
            }
        }

        private void SetScale(double value, bool animation = false)
        {
            if (AnimationTime == TimeSpan.Zero) animation = false;
            ScaleBorder.ScaleX = value;
            ScaleBorder.ScaleY = value;

            var sb = new Storyboard();
            _currentScale = value;
            if (animation)
            {
                var sx = new DoubleAnimation
                {
                    To = value,
                    EasingFunction = CircleEaseOut,
                    BeginTime = TimeSpan.Zero,
                    Duration = new Duration(AnimationTime)
                };
                var sy = sx.Clone();

                Storyboard.SetTargetProperty(sx, new PropertyPath("RenderTransform.Children[1].ScaleX"));
                Storyboard.SetTargetProperty(sy, new PropertyPath("RenderTransform.Children[1].ScaleY"));
                Storyboard.SetTarget(sx, Image);
                Storyboard.SetTarget(sy, Image);
                sb.Children.Add(sx);
                sb.Children.Add(sy);
                sb.Completed += (obj, arg) =>
                {
                    sb.Stop();
                    Scale.ScaleX = value;
                    Scale.ScaleY = value;
                };
                sb.Begin();
            }
            else
            {
                Scale.ScaleX = value;
                Scale.ScaleY = value;
            }
        }

        private void SetTranslate(double x, double y, bool animation = false)
        {
            if (AnimationTime == TimeSpan.Zero) animation = false;
            bool recLeft = false, recRight = false, recUp = false, recBottom = false;
            if (CurrentScale * SourceSize.Width >= ActualWidth)
            {
                if (x >= 0)
                {
                    x = 0;
                    if (_isMouseDown)
                    {
                        recLeft = true;
                        ShowBound(RecLeft);
                    }
                }
                else if ((x + SourceSize.Width) * CurrentScale <= ActualWidth)
                {
                    x = ActualWidth / CurrentScale - SourceSize.Width;
                    if (_isMouseDown)
                    {
                        recRight = true;
                        ShowBound(RecRight);
                    }
                }
            }
            else
            {
                var display = SourceSize.Width * CurrentScale;
                x = (ActualWidth - display) / (2 * CurrentScale);
            }

            if (CurrentScale * SourceSize.Height >= ActualHeight)
            {
                if (y >= 0)
                {
                    y = 0;
                    if (_isMouseDown)
                    {
                        recUp = true;
                        ShowBound(RecUp);
                    }
                }
                else if ((y + SourceSize.Height) * CurrentScale <= ActualHeight)
                {
                    y = ActualHeight / CurrentScale - SourceSize.Height;
                    if (_isMouseDown)
                    {
                        recBottom = true;
                        ShowBound(RecBottom);
                    }
                }
            }
            else
            {
                var display = SourceSize.Height * CurrentScale;
                y = (ActualHeight - display) / (2 * CurrentScale);
            }

            TranslateBorder.X = x;
            TranslateBorder.Y = y;

            if (Math.Abs(Translate.X - x) >= 0.01 || Math.Abs(Translate.Y - y) >= 0.01)
            {
                if (!recLeft) ClearBound(RecLeft);
                if (!recUp) ClearBound(RecUp);
                if (!recRight) ClearBound(RecRight);
                if (!recBottom) ClearBound(RecBottom);
            }

            if (animation)
            {
                var sb = new Storyboard();
                var mx = new DoubleAnimation
                {
                    To = x,
                    EasingFunction = CircleEaseOut,
                    BeginTime = TimeSpan.Zero,
                    Duration = new Duration(AnimationTime)
                };
                var my = new DoubleAnimation
                {
                    To = y,
                    EasingFunction = CircleEaseOut,
                    BeginTime = TimeSpan.Zero,
                    Duration = new Duration(AnimationTime)
                };
                Storyboard.SetTargetProperty(mx, new PropertyPath("RenderTransform.Children[0].X"));
                Storyboard.SetTargetProperty(my, new PropertyPath("RenderTransform.Children[0].Y"));
                Storyboard.SetTarget(mx, Image);
                Storyboard.SetTarget(my, Image);
                sb.Children.Add(mx);
                sb.Children.Add(my);
                sb.Completed += (obj, arg) =>
                {
                    sb.Stop();
                    Translate.X = x;
                    Translate.Y = y;
                };
                sb.Begin();
            }
            else
            {
                Translate.X = x;
                Translate.Y = y;
            }
        }

        private void ShowBound(Rectangle rec)
        {
            if (rec == null || _boundSbList.Contains(rec))
                return;

            var recTrans = (ScaleTransform)rec.RenderTransform;
            var sb = new Storyboard();
            var mx = new DoubleAnimation
            {
                To = 1,
                EasingFunction = CircleEaseOut,
                BeginTime = TimeSpan.Zero,
                Duration = new Duration(TimeSpan.FromMilliseconds(400))
            };

            string path;
            switch (rec.Name)
            {
                case nameof(RecLeft):
                case nameof(RecRight):
                    path = "X";
                    break;
                case nameof(RecUp):
                case nameof(RecBottom):
                    path = "Y";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Storyboard.SetTargetProperty(mx, new PropertyPath("RenderTransform.Scale" + path));
            Storyboard.SetTarget(mx, rec);
            sb.Children.Add(mx);
            sb.Completed += (obj, arg) =>
            {
                if (path == "X")
                    recTrans.ScaleX = 1;
                else
                    recTrans.ScaleY = 1;

                sb.Stop();
            };
            _boundSbList.Add(rec);
            sb.Begin();
        }

        private void ClearBound(Rectangle rec)
        {
            if (rec == null || !_boundSbList.Contains(rec))
                return;
            var recTrans = (ScaleTransform)rec.RenderTransform;
            var sb = new Storyboard();
            var mx = new DoubleAnimation
            {
                To = 0,
                EasingFunction = CircleEaseOut,
                BeginTime = TimeSpan.Zero,
                Duration = new Duration(TimeSpan.FromMilliseconds(400))
            };

            string path;
            switch (rec.Name)
            {
                case nameof(RecLeft):
                case nameof(RecRight):
                    path = "X";
                    break;
                case nameof(RecUp):
                case nameof(RecBottom):
                    path = "Y";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Storyboard.SetTargetProperty(mx, new PropertyPath("RenderTransform.Scale" + path));
            Storyboard.SetTarget(mx, rec);
            sb.Children.Add(mx);
            sb.Completed += (obj, arg) =>
            {
                if (path == "X")
                    recTrans.ScaleX = 0;
                else
                    recTrans.ScaleY = 0;

                sb.Stop();
                _boundSbList.Remove(rec);
            };
            _boundSbList.Add(rec);
            sb.Begin();
        }
    }
}
