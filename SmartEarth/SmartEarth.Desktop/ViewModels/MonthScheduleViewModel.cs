using Prism.Commands;
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
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class MonthScheduleViewModel : BindableBase, IViewAware
    {
        #region Properties

        IView _view;
        public IView View
        {
            get => _view;
            set
            {
                if (_view != null) _view.IsVisibleChanged -= ViewVisibleChanged;
                if (value != null) value.IsVisibleChanged += ViewVisibleChanged;
                _view = value;
            }
        }

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        IScheduler Scheduler { get; }
        #endregion

        #region Bindables
        public Calender Schedule => Scheduler.Schedule;
        #endregion

        #region Commands
        public ICommand ViewDayCommand { get; }
        public ICommand ViewSelectedDayCommand { get; }
        public ICommand MouseOverDayCommand { get; }
        public ICommand MouseLeaveDayCommand { get; }
        public ICommand SelectDayCommand { get; }

        public ICommand SelectUpCommand { get; }
        public ICommand SelectDownCommand { get; }
        public ICommand SelectLeftCommand { get; }
        public ICommand SelectRightCommand { get; }
        public ICommand ScrollDayUpCommand { get; }
        public ICommand ScrollDayDownCommand { get; }

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        #endregion

        #region Internals
        DependencyPropertyChangedEventHandler ViewVisibleChanged;
        #endregion

        #endregion

        #region Constructors
        public MonthScheduleViewModel(ILoggerFacade logger, IRegionManager regionManager, IScheduler scheduler)
        {
            Logger = logger;
            RegionManager = regionManager;
            Scheduler = scheduler;

            ViewDayCommand = new DelegateCommand<object>(OnViewDay);
            MouseOverDayCommand = new DelegateCommand<object>(OnMouseOverDay);
            MouseLeaveDayCommand = new DelegateCommand<object>(OnMouseLeaveDay);
            SelectDayCommand = new DelegateCommand<object>(OnSelectDay);

            SelectUpCommand = new DelegateCommand(OnSelectUp);
            SelectDownCommand = new DelegateCommand(OnSelectDown);
            SelectLeftCommand = new DelegateCommand(OnSelectLeft);
            SelectRightCommand = new DelegateCommand(OnSelectRight);

            ScrollDayUpCommand = new DelegateCommand(OnScrollDayUp);
            ScrollDayDownCommand = new DelegateCommand(OnScrollDayDown);
            ViewSelectedDayCommand = new DelegateCommand(OnViewSelectedDay);

            PreviousMonthCommand = new DelegateCommand(OnPrevious);
            NextMonthCommand = new DelegateCommand(OnNext);

            Scheduler.Loaded += (s, e) =>
            {
                RaisePropertyChanged(nameof(Schedule));
            };

            ViewVisibleChanged = (s, e) =>
            {
                if (View.IsVisible) View.Focus();
            };
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnViewDay(object obj)
        {
            if (!(obj is Day)) return;

            // TODO: Make sure the day is not over the limit
            Day day = (Day)obj;
            if (day != null && (day.Year < Schedule.StartYear || day.Year > Schedule.EndYear)) return;
            Schedule.SelectedDay = day;
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.DAY_SCHEDULE_VIEW);
        }

        void OnViewSelectedDay()
        {
            RegionManager.RequestNavigateToView(Core.SCHEDULE_REGION, Core.DAY_SCHEDULE_VIEW);
        }

        void OnMouseOverDay(object obj)
        {

        }

        void OnSelectDay(object obj)
        {
            if ((obj is Day))
            {
                var day = (Day)obj;
                Schedule.SelectedDay = day;
                View.Focus();
            }
            else if (obj is FrameworkElement)
            {
                var element = (FrameworkElement)obj;
                element.Focus();
                if (element.DataContext is Day)
                {
                    var day = (Day)element.DataContext;
                    Schedule.SelectedDay = day;
                }
            }
        }

        void OnMouseLeaveDay(object obj)
        {
            if (!(obj is Day)) return;
            var day = (Day)obj;

            //if (day == Schedule.SelectedDay) Schedule.SelectedDay = null;
        }

        void OnSelectLeft()
        {
            int index = Schedule.CurrentView.IndexOf(Schedule.SelectedDay);
            if (index > 0 && Schedule.Context == CalenderView.Month)
                Schedule.SelectedDay = (Day)Schedule.CurrentView[index - 1];
        }

        void OnSelectRight()
        {
            int index = Schedule.CurrentView.IndexOf(Schedule.SelectedDay);
            if (index < Schedule.CurrentView.Count - 1 && Schedule.Context == CalenderView.Month)
                Schedule.SelectedDay = (Day)Schedule.CurrentView[index + 1];
        }

        void OnSelectUp()
        {
            int newIndex = Schedule.CurrentView.IndexOf(Schedule.SelectedDay) - 7;
            if (newIndex > 0 && Schedule.Context == CalenderView.Month)
                Schedule.SelectedDay = (Day)Schedule.CurrentView[newIndex];
        }

        void OnSelectDown()
        {
            int newIndex = Schedule.CurrentView.IndexOf(Schedule.SelectedDay) + 7;
            if (newIndex < Schedule.CurrentView.Count - 1 && Schedule.Context == CalenderView.Month)
                Schedule.SelectedDay = (Day)Schedule.CurrentView[newIndex];
        }

        void OnScrollDayUp()
        {
            Schedule.SelectedDay.ViewOffsetY -= 10;
        }

        void OnScrollDayDown()
        {
            Schedule.SelectedDay.ViewOffsetY += 10;
        }

        void OnPrevious()
        {
            if (Schedule.ViewIsLoaded) Task.Run(() => Schedule.Previous());
        }

        void OnNext()
        {
            if (Schedule.ViewIsLoaded) Task.Run(() => Schedule.Next());
        }
        #endregion

        #endregion
    }
}
