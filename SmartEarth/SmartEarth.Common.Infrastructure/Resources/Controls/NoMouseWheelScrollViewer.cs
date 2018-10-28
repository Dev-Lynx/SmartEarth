using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEarth.Common.Infrastructure.Resources.Controls
{
    /// <summary>
    /// ScrollViewer Control without mouse scrolling.
    /// </summary>
    public class NoMouseWheelScrollViewer : ScrollViewer
    {  
        #region Methods

        #region Overrides
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            base.OnPreviewMouseWheel(e);

            if (!(Content is FrameworkElement)) return;

            var child = (FrameworkElement)Content;
            var childEvent = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent, Source = child
            };
            child.RaiseEvent(childEvent);
        }
        #endregion

        #endregion
    }
}
