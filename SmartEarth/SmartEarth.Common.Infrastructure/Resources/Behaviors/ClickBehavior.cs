using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace SmartEarth.Common.Infrastructure.Resources.Behaviors
{
    public class ClickBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true;
                AssociatedObject.CaptureMouse();
            };
            AssociatedObject.MouseLeftButtonUp += (s, e) =>
            {
                if (!AssociatedObject.IsMouseCaptured) return;
                e.Handled = true;
                AssociatedObject.ReleaseMouseCapture();
                if (AssociatedObject.InputHitTest(e.GetPosition(AssociatedObject)) != null)
                    AssociatedObject.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            };
        }
    }
}
