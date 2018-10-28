using SmartEarth.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Core.Initialize();

            this.Dispatcher.UnhandledException += (s, ex) => Core.Log.Error("An unhandled exception occured in the dispatcher\n{0}", ex.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, ex) => Core.Log.Error("An unhandled exception occured in the app domain\n{0}", ex.ExceptionObject);
            this.DispatcherUnhandledException += (s, ex) => Core.Log.Error(ex.Exception, "An unhandled exception occured in the application\n{0}", ex.Exception);
            TaskScheduler.UnobservedTaskException += (s, ex) => Core.Log.Error(ex.Exception, "An unhandled exception occured in the task scheduler\n{0}", ex.Exception);

            new Bootstrapper().Run();
        }
    }
}
