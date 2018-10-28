using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Resources.Interfaces
{
    public interface IWindow
    {
        bool IsLoaded { get; }
        event RoutedEventHandler Loaded;
        double ActualWidth { get; }
        double ActualHeight { get; }
        double Width { get; set; }
        double Height { get; set; }
        double Left { get; set; }
        double Top { get; set; }
        bool Topmost { get; set; }
        WindowState WindowState { get; set; }
        bool Activate();
        void DragMove();
        void Close();
    }
}
