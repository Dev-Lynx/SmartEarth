using MaterialDesignThemes.Wpf.Transitions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using SmartEarth.Common.Infrastructure.Services;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Transitionals.Controls;

namespace SmartEarth.Desktop
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override ILoggerFacade CreateLogger() => new NLogger();

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<IConfigurationManager, XMLConfigurationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDatabaseManager, LiteDatabaseManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITaskManager, TaskManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScheduler, QuartzSheduler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IViewManager, ViewManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowService, WindowService>(new ContainerControlledLifetimeManager());       
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemmbleName = viewType.Assembly.FullName;
                var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", viewName, suffix);

                var assembly = viewType.Assembly;
                var type = assembly.GetType(viewModelName, true);

                return type;
            });
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(TransitionElement), Container.Resolve<TransitionElementRegionAdapter>());
            mappings.RegisterMapping(typeof(TransitioningContent), Container.Resolve<MaterialTransitionRegionAdapter>());
            return mappings;
        }

        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.TryResolve<Views.Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }
    }
}
