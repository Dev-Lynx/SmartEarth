using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels 
{
    public class ScheduleViewModel : BindableBase, INavigationAware
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        public IScheduler Scheduler { get; }
        #endregion

        #region Bindables
        public Calender Schedule => Scheduler.Schedule;
        public bool MonthViewActive => CurrentView == Core.MONTH_SCHEDULE_VIEW;

        public string CurrentWeekDay
        {
            get => Schedule.SelectedDay;
            set
            {
                int diff = Calender.DaysOfTheWeek.IndexOf(value.ToString()) - Calender.DaysOfTheWeek.IndexOf(CurrentWeekDay.ToString());
                Day current = Schedule.CurrentDay;
                Day day = new Day(current.Month, current.Year, current + diff);
                Schedule.SetDate(day);
                RaiseDateChanged();
            }
        }

        public int CurrentDayIndex
        {
            get
            {
                var index = Schedule.SelectedDay - 1;
                Logger.Debug("Index Update -- {0}", index);
                return 0;
            }
            set
            {
                if (value < 0) return;
                Day current = Schedule.SelectedDay;
                Day day = new Day(current.Month, current.Year, value + 1);
                Schedule.SetDate(day);
                RaiseDateChanged();
            }
        }

        public ObservableCollection<Day> Days => Schedule.CurrentMonth.Days;

        bool firstDayAttempt = true;
        public Day CurrentDay
        {
            get => Schedule.SelectedDay;
            set
            {
                if (firstDayAttempt)
                {
                    firstDayAttempt = false;
                    return;
                }
                if (value == null) return;
                Schedule.SetDate(value);
                RaiseDateChanged();
            }
        }

        public string CurrentMonth
        {
            get => Schedule.SelectedDay.Month.ToString();
            set
            {
                int diff = Calender.MonthsOfTheYear.IndexOf(value.ToString()) - Calender.MonthsOfTheYear.IndexOf(Schedule.CurrentMonth.ToString());
                Month current = Schedule.CurrentMonth;
                Month month = new Month(current.Year, current + diff);
                Schedule.SetDate(month);
                RaiseDateChanged();
            }
        }

        bool firstAttempt = true;
        public Year CurrentYear
        {
            get => Schedule.SelectedDay.Year;
            set
            {
                if (firstAttempt && value == Schedule.StartYear)
                {
                    firstAttempt = false;
                    return;
                }
                else if (firstAttempt) firstAttempt = false;

                Schedule.SetDate(value);
                RaiseDateChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        #endregion

        #region Internals
        string CurrentView { get; set; } = Core.MONTH_SCHEDULE_VIEW;
        #endregion

        #endregion

        #region Constructors
        public ScheduleViewModel(ILoggerFacade logger, IScheduler scheduler)
        {
            Logger = logger;
            Scheduler = scheduler;

            PreviousCommand = new DelegateCommand(OnPrevious);
            NextCommand = new DelegateCommand(OnNext);

            Scheduler.Loaded += (s, e) =>
            {
                RaisePropertyChanged(nameof(Schedule));
            };

            UIHelper.ViewChanged += (s, e) =>
            {
                if (e.Region == Core.SCHEDULE_REGION)
                    CurrentView = e.View;
                RaisePropertyChanged(nameof(MonthViewActive));
            };
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnPrevious()
        {
            Task.Run(async () => await Schedule.Previous());
        }
        void OnNext()
        {
            Task.Run(async () => await Schedule.Next());
        }

        void OnDayChanged()
        {
            
        }


        #region INavigationAware Implementation
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Load();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // TODO: Maybe, Dispose the calender?   
        }

        async void Load()
        {
            //IsBusy = true;
            await Task.Run(async () => 
            {
                await Scheduler.Load();
            });
            RaiseDateChanged();
            Schedule.DateChanged += (s, e) => RaiseDateChanged();
            //IsBusy = false;

        }
        #endregion

        void RaiseDateChanged()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RaisePropertyChanged(nameof(CurrentYear));
                RaisePropertyChanged(nameof(CurrentMonth));
                RaisePropertyChanged(nameof(Days));
                RaisePropertyChanged(nameof(CurrentDayIndex));
                RaisePropertyChanged(nameof(CurrentDay));
                RaisePropertyChanged(nameof(CurrentWeekDay));
            });
        }

        #endregion

        #endregion
    }
}
