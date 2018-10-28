using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class DayScheduleViewModel : BindableBase, INavigationAware
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        IScheduler Scheduler { get; }
        #endregion

        #region Bindables
        public Calender Schedule => Scheduler.Schedule;
        #endregion

        #region Commands
        public ICommand BackCommand { get; }
        public ICommand NewTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        #endregion 

        #endregion

        #region Constructors
        public DayScheduleViewModel(ILoggerFacade logger, IRegionManager regionManager, IScheduler scheduler)
        {
            Logger = logger;
            RegionManager = regionManager; 
            Scheduler = scheduler;

            BackCommand = new DelegateCommand(OnBack);
            NewTaskCommand = new DelegateCommand(OnNewTask);
            EditTaskCommand = new DelegateCommand<object>(OnEditTask);
        }
        #endregion

        #region Methods

        #region INavigationAware Implementation
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Schedule.CurrentDay.BuildHours();
            RaisePropertyChanged(nameof(Schedule));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) => Schedule.CurrentDay.DemolishHours();
        #endregion

        #region Command Handlers
        void OnBack()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.MONTH_SCHEDULE_VIEW);
        }

        void OnNewTask()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.NEW_TASK_VIEW);
        }

        void OnEditTask(object obj)
        {
            if (!(obj is GEAutomationTask)) return;
            var task = (GEAutomationTask)obj;

            NavigationParameters parameters = new NavigationParameters();
            parameters.Add("Task", task);
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.NEW_TASK_VIEW, parameters);
        }
        #endregion

        #endregion
    }
}
