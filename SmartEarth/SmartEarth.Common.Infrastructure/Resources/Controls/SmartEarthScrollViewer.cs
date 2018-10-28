using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Resources.Controls
{
    public class SmartEarthScrollViewer : NoMouseWheelScrollViewer
    {
        #region Properties
        public double DynamicHorizontalOffset
        {
            get { return (double)GetValue(DynamicHorizontalOffsetProperty); }
            set { SetValue(DynamicHorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty DynamicHorizontalOffsetProperty =
            DependencyProperty.Register("DynamicHorizontalOffset", typeof(double), typeof(SmartEarthScrollViewer));

        public double DynamicVerticalOffset
        {
            get { return (double)GetValue(DynamicVerticalOffsetProperty); }
            set { SetValue(DynamicVerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty DynamicVerticalOffsetProperty =
            DependencyProperty.Register("DynamicVerticalOffset", typeof(double), typeof(SmartEarthScrollViewer));

        public double DynamicViewportWidth
        {
            get { return (double)GetValue(DynamicViewportWidthProperty); }
            set { SetValue(DynamicViewportWidthProperty, value); }
        }

        public static readonly DependencyProperty DynamicViewportWidthProperty =
            DependencyProperty.Register("DynamicViewportWidth", typeof(double), typeof(SmartEarthScrollViewer));

        public double DynamicViewportHeight
        {
            get { return (double)GetValue(DynamicViewportHeightProperty); }
            set { SetValue(DynamicViewportHeightProperty, value); }
        }

        public static readonly DependencyProperty DynamicViewportHeightProperty =
            DependencyProperty.Register("DynamicViewportHeight", typeof(double), typeof(SmartEarthScrollViewer));



        public double DynamicScrollableWidth
        {
            get { return (double)GetValue(DynamicScrollableWidthProperty); }
            set { SetValue(DynamicScrollableWidthProperty, value); }
        }

        public static readonly DependencyProperty DynamicScrollableWidthProperty =
            DependencyProperty.Register("DynamicScrollableWidth", typeof(double), typeof(SmartEarthScrollViewer));

        public double DynamicScrollableHeight
        {
            get { return (double)GetValue(DynamicScrollableHeightProperty); }
            set { SetValue(DynamicScrollableHeightProperty, value); }
        }

        public static readonly DependencyProperty DynamicScrollableHeightProperty =
            DependencyProperty.Register("DynamicScrollableHeight", typeof(double), typeof(SmartEarthScrollViewer));


        #endregion

        #region Methods
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DynamicVerticalOffsetProperty)
            {
                if (ScrollInfo != null)
                    ScrollInfo.SetVerticalOffset(DynamicVerticalOffset);
            }
            else if (e.Property == DynamicHorizontalOffsetProperty)
            {
                if (ScrollInfo != null)
                    ScrollInfo.SetHorizontalOffset(DynamicHorizontalOffset);
            }
            else if (e.Property == HorizontalOffsetProperty)
            {
                DynamicHorizontalOffset = (double)e.NewValue;
            }
            else if (e.Property == VerticalOffsetProperty)
            {
                DynamicVerticalOffset = (double)e.NewValue;
            }
            else if (e.Property == ViewportWidthProperty)
            {
                DynamicViewportWidth = (double)e.NewValue;
            }
            else if (e.Property == ViewportHeightProperty)
            {
                DynamicViewportHeight = (double)e.NewValue;
            }
            else if (e.Property == ScrollableWidthProperty)
                DynamicScrollableWidth = (double)e.NewValue;
            else if (e.Property == ScrollableHeightProperty)
                DynamicScrollableHeight = (double)e.NewValue;

        }
        #endregion
    }
}
