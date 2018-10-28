using Microsoft.Practices.Unity;
using Prism.Logging;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Controls;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class WindowService : IWindowService
    {
        #region Properties

        #region Services
        IUnityContainer Container { get; }
        ILoggerFacade Logger { get; }
        #endregion

        public List<IWindow> Windows { get; } = new List<IWindow>();
        #endregion

        #region Constructors
        public WindowService(ILoggerFacade logger, IUnityContainer container)
        {
            Container = container;
            Logger = logger;
        }
        #endregion

        #region Methods

        #region IWindowService Implementation
        public IWindow CreateWindow(WindowCreationSettings settings)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                SmartEarthWindow window = Container.Resolve<SmartEarthWindow>();
                window.DataContext = settings.DataContext;
                if (settings.Style != null) window.Style = settings.Style;
                
                window.Topmost = settings.Topmost;
                window.WindowStartupLocation = settings.StartupLocation;
                window.SizeToContent = settings.SizeToContent;

                if (!string.IsNullOrWhiteSpace(settings.Title))
                    window.Title = settings.Title;

                if (settings.DataContext is IShellAware)
                    ((IShellAware)settings.DataContext).Load(window);

                if (settings.IsModal) window.ShowDialog();
                else window.Show();


                Windows.Add(window);
                return window;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Usercontrol type of the window</typeparam>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<IWindow> CreateWindow<T>(WindowCreationSettings settings) where T : class
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SmartEarthWindow window = Container.Resolve<SmartEarthWindow>();
                object content = Container.Resolve<T>();
                window.Content = content;

                if (settings.DataContext != null)
                    window.DataContext = settings.DataContext;

                if (settings.Style != null) window.Style = settings.Style;

                window.Topmost = settings.Topmost;
                window.WindowStartupLocation = settings.StartupLocation;
                settings.SizeToContent = settings.SizeToContent;

                if (!string.IsNullOrWhiteSpace(settings.Title))
                    window.Title = settings.Title;

                if (settings.DataContext is IShellAware)
                    ((IShellAware)settings.DataContext).Load(window);
                else if (content is FrameworkElement && ((FrameworkElement)content).DataContext is IShellAware)
                    ((IShellAware)((FrameworkElement)content).DataContext).Load(window);

                RoutedEventHandler loaded = null;
                window.Loaded += loaded = (s, e) =>
                {
                    window.Loaded -= loaded;
                    if (!settings.RestrictResizing) return;

                    Rect rect = new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight);
                    window.SizeToContent = SizeToContent.Manual;

                    window.Left = rect.Left;
                    window.Top = rect.Top;
                    window.Width = rect.Width;
                    window.Height = rect.Height;
                };


                if (settings.IsModal) window.ShowDialog();
                else window.Show();

                Windows.Add(window);
                return window;
            });
        }

        public void DestroyWindow(IWindow window)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Windows.Remove(window);
                window.Close();
            });
        }
        #endregion

        #endregion
    }
}
