using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class MenuViewModel : BindableBase
    {
        #region Properties

        #region Services
        IRegionManager RegionManager { get; }
        ILoggerFacade Logger { get; }
        IWindowService WindowService { get; }
        IScheduler Scheduler { get; }
        #endregion

        #region Commands
        public ICommand StartAutomationCommand { get; }
        public ICommand ScheduleCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand ViewScheduleCommand { get; }


        public ICommand TogglePendingTasksCommand { get; }
        public ICommand ToggleCompletedTasksCommand { get; }
        #endregion

        public InteractionRequest<INotification> NewAutomationRequest { get; } = new InteractionRequest<INotification>();
        public InteractionRequest<INotification> ViewProgressRequest { get; } = new InteractionRequest<INotification>();

        #region Bindables
        bool _viewerActive = false;
        public bool ViewerActive { get => _viewerActive; set => SetProperty(ref _viewerActive, value); }

        public bool CalenderView => ActiveView == Core.MONTH_SCHEDULE_VIEW || ActiveView == Core.SCHEDULE_VIEW || ActiveView == Core.RECENT_VIEW;
        public bool DayView => ActiveView == Core.DAY_SCHEDULE_VIEW;
        public bool NewTaskView => ActiveView == Core.NEW_TASK_VIEW;
        #endregion

        #region Internals
        string ActiveRegion { get; set; }
        string ActiveView { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MenuViewModel(ILoggerFacade logger, IRegionManager regionManager, IScheduler scheduler, IWindowService windowService)
        {
            Logger = logger;
            WindowService = windowService;
            RegionManager = regionManager;
            Scheduler = scheduler;

            StartAutomationCommand = new DelegateCommand(OnStartAutomation);
            ScheduleCommand = new DelegateCommand(OnViewSchedule);
            SelectionChangedCommand = new DelegateCommand<object>(OnSelectionChanged);
            ViewScheduleCommand = new DelegateCommand(OnViewSchedule);

            UIHelper.ViewChanged += (s, e) =>
            {
                ActiveRegion = e.Region;
                ActiveView = e.View;

                switch (e.Region)
                {
                    case Core.HOME_REGION:
                        ViewerActive = e.View == Core.IMAGE_VIEW;
                        break;

                    case Core.MAIN_REGION:
                        RegionManager.RequestNavigateToView(Core.HOME_REGION, Core.RECENT_VIEW);
                        ViewerActive = false;
                        break;
                }

                RaisePropertyChanged(nameof(CalenderView));
                RaisePropertyChanged(nameof(DayView));
                RaisePropertyChanged(nameof(NewTaskView));
            };
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnStartAutomation()
        {
            //NewAutomationRequest.Raise(new Notification() { Title = "New Automation", Content = "Hello" });
            WindowService.CreateWindow<Views.NewAutomationView>(new WindowCreationSettings()
            {
                IsModal = true, Style = (Style)Application.Current.Resources["ModalWindow"], StartupLocation = WindowStartupLocation.CenterScreen,
                Title = (string)Application.Current.Resources["MODAL_NEW_AUTOMATION_TITLE"], RestrictResizing = true, SizeToContent = SizeToContent.WidthAndHeight
            });
        }

        void OnViewSchedule()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.DAY_SCHEDULE_VIEW);
        }

        void OnSelectionChanged(object obj)
        {
            if (!(obj is INavigationTab)) return;

            var tab = ((INavigationTab)obj);

            RegionManager.RequestNavigateToView(tab.Region, tab.View);
        }
        #endregion

        #endregion
    }
}
