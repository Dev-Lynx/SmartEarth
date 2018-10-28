using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class NewAutomationViewModel : BindableBase, INavigationAware, IShellAware
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IConfigurationManager ConfigurationManager { get; }
        IScheduler Scheduler { get; }
        IWindowService WindowService { get; }
        #endregion

        #region Bindables
        GEAutomationTask _geAutomationTask = new GEAutomationTask();
        public GEAutomationTask Task { get => _geAutomationTask; private set => SetProperty(ref _geAutomationTask, value); }
        public Configuration Configuration => ConfigurationManager.Configuration;
        public Location Location => Task.Location;
        public IWindow Window { get; private set; }
        #endregion

        #region Commands
        public ICommand StartAutomationCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region Internals
        bool IsLoaded { get; set; }
        Random Random { get; } = new Random();
        #endregion

        #endregion

        #region Constructors
        public NewAutomationViewModel(ILoggerFacade logger, IWindowService windowService, IConfigurationManager configurationManager, IScheduler scheduler)
        {
            Logger = logger;
            ConfigurationManager = configurationManager;
            Scheduler = scheduler;
            WindowService = windowService;

            StartAutomationCommand = new DelegateCommand(OnStartAutomation);
            CancelCommand = new DelegateCommand(OnCancel);
        }
        #endregion

        #region Methods

        #region INavigationAware Implementation
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Task = new GEAutomationTask();
            Task.Color = Configuration.Colors[Random.Next(Configuration.Colors.Count)];
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion

        #region IShellAware Implementation
        public void Load(IWindow window)
        {
            if (IsLoaded) return;
            Window = window;
            //Task = new GEAutomationTask();
            Task.Color = Configuration.Colors[Random.Next(Configuration.Colors.Count)];
        }
        #endregion

        #region Command Handlers
        void OnStartAutomation()
        {
            Task.Due = DateTime.Now;
            Scheduler.StartNow(Task);
            WindowService.DestroyWindow(Window);
        }

        void OnCancel()
        {
            WindowService.DestroyWindow(Window);
        }

        
        #endregion

        #endregion
    }
}
