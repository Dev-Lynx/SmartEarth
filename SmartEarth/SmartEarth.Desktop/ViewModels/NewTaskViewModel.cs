using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Commands;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class NewTaskViewModel : BindableBase, INavigationAware
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        IScheduler Scheduler { get; }
        IConfigurationManager ConfigurationManager { get; }
        #endregion

        #region Commands
        public ICommand CancelCommand { get; }
        public ICommand ScheduleCommand { get; }
        public ICommand SelectColorCommand { get; }
        #endregion

        #region Bindables
        GEAutomationTask _geAutomationTask = new GEAutomationTask();
        public GEAutomationTask Task { get => _geAutomationTask; private set => SetProperty(ref _geAutomationTask, value); }
        DateTime _startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.AddHours(1).Hour, 0, 0);
        public DateTime StartTime { get => _startTime; set => SetProperty(ref _startTime, value); }
        public RepeatInterval Interval => Task.Interval;
        public Configuration Configuration => ConfigurationManager.Configuration;

        public int RepetitionIndex
        {
            get => (int)Interval.Repetition;
            set { Task.Interval.Repetition = (Repetition)value; RaisePropertyChanged(nameof(Interval)); RaisePropertyChanged(nameof(RepetitionIndex)); }
        }

        public int ExpirationIndex
        {
            get => (int)Interval.Expiration;
            set { Interval.Expiration = (ExpirationType)value; RaisePropertyChanged(nameof(Interval)); RaisePropertyChanged(); }
        }
        #endregion

        #region Internals
        bool IsNew { get; set; }
        #endregion

        #endregion

        #region Constructors
        public NewTaskViewModel(ILoggerFacade logger, IRegionManager regionManager, IScheduler scheduler, IConfigurationManager configurationManager)
        {
            Logger = logger;
            RegionManager = regionManager;
            Scheduler = scheduler;
            ConfigurationManager = configurationManager;

            ScheduleCommand = new DelegateCommand(OnSchedule);
            CancelCommand = new DelegateCommand(OnCancel);
            SelectColorCommand = new DelegateCommand(OnSelectColor);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnSelectColor()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.COLOR_PICKER_VIEW);
        }

        void OnSchedule()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.DAY_SCHEDULE_VIEW);

            DateTime date = Scheduler.Schedule.CurrentDay.Date;
            Task.Due = new DateTime(date.Year, date.Month, date.Day, StartTime.Hour, StartTime.Minute, StartTime.Second);
            if (string.IsNullOrWhiteSpace(Task.Color.Name)) Task.Color = Configuration.Colors.FirstOrDefault();
            Task.Heading = Task.Name;

            System.Threading.Tasks.Task.Run(async() => await Scheduler.ScheduleTask(Task));
        }

        void OnCancel()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.DAY_SCHEDULE_VIEW);
        }
        #endregion

        #region INavigationAware Implementation
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Task = (GEAutomationTask)navigationContext.Parameters["Task"];

            if (Task == null)
            {
                Random r = new Random();
                Task = new GEAutomationTask();
                Task.Color = Configuration.Colors[r.Next(Configuration.Colors.Count)];
                StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.AddHours(1).Hour, 0, 0);
            }
            else StartTime = Task.Due;


            RaisePropertyChanged(nameof(Task));
            RaisePropertyChanged(nameof(Interval));
            RaisePropertyChanged(nameof(StartTime));
            RaisePropertyChanged(nameof(ExpirationIndex));
            RaisePropertyChanged(nameof(RepetitionIndex));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion

        #endregion
    }
}
