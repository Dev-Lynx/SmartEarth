using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SmartEarth.Common.Infrastructure.Extensions
{
    public class ViewEventArgs : EventArgs
    {
        public string View { get; set; }
        public string Region { get; set; }

        public ViewEventArgs(string view, string region)
        {
            View = view;
            Region = region;
        }
    }



    public static class UIHelper
    {
        #region Properties
        public delegate void ViewEventHandler(object sender, ViewEventArgs args);
        public static event ViewEventHandler ViewChanged;
        #endregion

        #region Methods

        public static void RequestNavigateToView(this IRegionManager manager, string region, string view, NavigationParameters parameters = null)
        {
            try
            {
                var control = (System.Windows.Controls.UserControl)manager.Regions[region].GetView(view);
                if (control == null)
                    throw new NullReferenceException("The region manager could not locate the specified region or view.");

                if (parameters == null) parameters = new NavigationParameters();

                Application.Current.Dispatcher.Invoke(() => manager.RequestNavigate(region, control.GetType().Name, parameters), System.Windows.Threading.DispatcherPriority.Send);
                ViewChanged?.Invoke(null, new ViewEventArgs(view, region));
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex);
            }
        }

        public static T FindAncestor<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            try
            {
                var parent = VisualTreeHelper.GetParent(dependencyObject);

                if (parent == null) return null;

                var parentT = parent as T;
                return parentT ?? parent.FindAncestor<T>();
            }
            catch { return null; }
        }

        #region Animations
        public static async Task DoubleAnimation(this FrameworkElement element, DependencyProperty property, double from, double to, Duration duration)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from, 
                To = to, 
                FillBehavior = FillBehavior.Stop,
                Duration = duration
            };

            EventHandler handler = null;
            bool completed = false;

            animation.Completed += handler = (s, e) => completed = true;

            element.BeginAnimation(property, animation);

            while (!completed) await Task.Delay(10);
            animation.Completed -= handler;
        }

        public static async Task FadeOut(this FrameworkElement element, double speed = 1, bool reappear = false)
        {
            if (speed == 0) speed = 1;

            if (element.Opacity <= 0)
                element.Opacity = 1;

            // time = distance / speed
            var time = (double)1 / speed;
            var from = element.Opacity;

            if (double.IsNaN(from) || double.IsInfinity(from))
                from = 1;

            var animation = new DoubleAnimation
            {
                From = from,
                To = 0,
                FillBehavior = FillBehavior.Stop,
                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(time))
            };

            EventHandler fadeHandler = null;
            bool completed = false;

            animation.Completed += fadeHandler = (s, e) =>
            {
                if (reappear) element.Opacity = 1;
                else element.Opacity = 0;
                completed = true;
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            while (!completed)
                await Task.Delay(10);
        }

        public static async Task FadeIn(this FrameworkElement element, double speed = 1, bool disappear = false)
        {
            if (speed <= 0) speed = 1;

            // time = distance / speed
            var time = 1.0 / speed;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Duration = new Duration(TimeSpan.FromSeconds(time))
            };

            EventHandler fadeHandler = null;
            bool completed = false;

            animation.Completed += fadeHandler = (s, e) =>
            {
                if (disappear) element.Opacity = 0;
                else element.Opacity = 1;
                completed = true;
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            while (!completed)
                await Task.Delay(10);
        }


        
        public static async Task AnimateHeight(this FrameworkElement element, double from, double value, double speed = 1, bool demolish = false)
        {
            if (speed <= 0) speed = 1;
            var time = 1.0 / speed;

            /*
            double oldHeight = element.ActualHeight;
            if (double.IsNaN(oldHeight)) oldHeight = 0;
            */

            Duration duration = new Duration(TimeSpan.FromSeconds(time));

            await Application.Current.Dispatcher.InvokeAsync(() => element.DoubleAnimation(FrameworkElement.HeightProperty, from, value, duration));

            if (demolish) element.Height = 0;
            else element.Height = value;
        }
        #endregion

        #region Colors
        public static Color GetColor(this BitmapSource source, Point point)
        {
            try
            {
                CroppedBitmap bitmap = new CroppedBitmap(source, new Int32Rect((int)Math.Round(point.X), (int)Math.Round(point.Y), 1, 1));
                byte[] pixels = new byte[4];
                bitmap.CopyPixels(pixels, pixels.Length, 0);
                return Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            }
            catch { return new Color(); }
        }

        public static SolidColorBrush ToSolidBrush(this string hex)
        {
            try
            {
                hex = hex.Replace("#", string.Empty);

                if (hex.Length < 8) hex = "FF" + hex;
                byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
                SolidColorBrush myBrush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
                return myBrush;
            }
            catch
            {
                return new SolidColorBrush();
            }
        }
        #endregion

        public static Cursor LoadCursor(string resourceName)
        {
            try
            {
                TextBlock block = (TextBlock)Application.Current.Resources[resourceName];
                return block.Cursor;
            }
            catch (Exception ex)
            {
                Core.Log.Error("An error ocurred while attempting to load cursor {0}", ex.Message);
                return Cursors.Arrow;
            }
        }

        #endregion
    }
}
