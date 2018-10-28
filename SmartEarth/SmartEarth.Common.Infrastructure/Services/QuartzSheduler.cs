using Prism.Logging;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Impl;
using static Quartz.MisfireInstruction;
using SmartEarth.Common.Infrastructure.Extensions;
using Prism.Mvvm;
using System.Windows;
using Minichmayr.Async;
using System.Diagnostics;
using SmartEarth.Common.Infrastructure.Models.Interfaces;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class QuartzSheduler : BindableBase, Interfaces.IScheduler, IWaitable
    {
        #region Properties

        #region Statics
        static readonly string LOADING_STATUS = (string)Core.Resources["CALENDER_LOADING"];
        static readonly string SYNCHRONIZING_STATUS = (string)Core.Resources["CALENDER_SYNCHRONIZING"];
        #endregion

        #region Events
        public event EventHandler Loaded;
        #endregion

        #region Services
        ILoggerFacade Logger { get; }
        IDatabaseManager DatabaseManager { get; }
        ITaskManager TaskManager { get; }
        #endregion

        #region Bindables
        Calender _schedule = null;
        public Calender Schedule { get => _schedule; private set => SetProperty(ref _schedule, value); }

        public bool UserWaiting { get; set; }

        bool _isBusy = true;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        string _status = (string)Core.Resources["CALENDER_LOADING"];
        public string Status { get => _status; private set => SetProperty(ref _status, value); } 
        #endregion

        #region Internals
        Quartz.IScheduler Scheduler { get; set; }

        bool _isLoaded = false;
        bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;

                if (_isLoaded) Loaded?.Invoke(this, EventArgs.Empty);
                // TODO: Unloaded?
            }
        }
        #endregion

        #endregion

        #region Constructors
        public QuartzSheduler(ILoggerFacade logger, IDatabaseManager databaseManager, ITaskManager taskManager)
        {
            Logger = logger;
            DatabaseManager = databaseManager;
            TaskManager = taskManager;

            Task.Run(() => Initialize());
        }
        #endregion

        #region Methods

        #region IScheduler Implementation
        public async Task ScheduleTask(SmartEarthTask task)
        {
            UpdateStatus(true, SYNCHRONIZING_STATUS);
            DatabaseManager.SaveScheduledTask(task);
            await AddToSchedule(task);

            if (task is IPresentable)
            {
                var tasks = await CloneForCalender((IPresentable)task, task.Interval).ToListAsync();
                await Schedule.SynchronizeTasks(tasks);
            }

            UpdateStatus();
        }

        public async void StartNow(SmartEarthTask task)
        {
            task.Due = DateTime.Now;
            var detail = task.CompileDetail(this);
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule().StartNow().Build();
            await Scheduler.ScheduleJob(detail, trigger);
            DatabaseManager.SaveScheduledTask(task);

            if (task is IPresentable)
                await Schedule.SynchronizeTasks((IPresentable)task);
        }

        async Task AddToSchedule(SmartEarthTask task)
        {
            // TODO: Log the task as a failure
            if (task.Due < DateTime.Now) return;

            var detail = task.CompileDetail(this);
            var dateOffset = DateBuilder.DateOf(task.Due.Hour
                , task.Due.Minute, task.Due.Second
                , task.Due.Day, task.Due.Month, task.Due.Year);
            
            
            var expireDate = task.Interval.ExpirationDate;
            var expireOffset = DateBuilder.DateOf(expireDate.Hour, expireDate.Minute
                , expireDate.Second, expireDate.Day, expireDate.Month, expireDate.Year);
            

            ICalendarIntervalTrigger trigger = null;

            if (task.Interval.ExpiresOnDate)
                trigger = (ICalendarIntervalTrigger)TriggerBuilder.Create().WithIdentity($"{task.Name}_{task.Interval.Index}", task.Signature.ToString()).WithCalendarIntervalSchedule().StartAt(dateOffset).EndAt(expireOffset).Build();
            else trigger = (ICalendarIntervalTrigger)TriggerBuilder.Create().WithIdentity($"{task.Name}_{task.Interval.Index}", task.Signature.ToString()).WithCalendarIntervalSchedule().StartAt(dateOffset).Build();
            
            if (task.Interval.IsEnabled)
            {
                trigger.RepeatInterval = task.Interval.Interval;
                trigger.RepeatIntervalUnit = (IntervalUnit)((int)task.Interval.Repetition + 3);
                if (trigger.RepeatIntervalUnit == IntervalUnit.Week)
                {
                    trigger.RepeatIntervalUnit = IntervalUnit.Day;
                    trigger.RepeatInterval = 1;
                }
            }
            
            await Scheduler.ScheduleJob(detail, trigger);
        }

        public void ImplementTask(SmartEarthTask task)
        {
            
            TaskManager.RegisterTask(task);
            /*
            if (task.Interval.IsEnabled)
            {
                task.Interval.Re
                if (!task.Interval.IsWeekly)
                {
                    
                }
            }
            */
        }

        public async Task Load()
        {
            if (IsLoaded) return;
            UpdateStatus(true, LOADING_STATUS);
            Schedule = await Calender.Build(Year.MinYear, Year.MaxYear);
            Schedule.Initialize();
            UpdateStatus(true, SYNCHRONIZING_STATUS);

            foreach (var task in DatabaseManager.ScheduledTasks)
                if (task is IPresentable)
                {
                    var list = await CloneForCalender((IPresentable)task, task.Interval).ToListAsync();
                    
                    //await CloneForCalender((IPresentable)task, task.Interval).ForEachAsync(t => Schedule.SynchronizeTasks(t));
                    await Schedule.SynchronizeTasks(list);
                }
            //await Schedule.SynchronizeTasks(DatabaseManager.ScheduledTasks.Where(t => t is IPresentable).Cast<IPresentable>());
            //await Schedule.SynchronizeTasks(DatabaseManager.CompletedTasks.Where(t => t is IPresentable).Cast<IPresentable>());
            Logger.Debug("Calender Load Complete...");
            IsLoaded = true;
            Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(Schedule)));
            UpdateStatus();
        }

        public Task Unload()
        {
            if (!IsLoaded) return Task.CompletedTask;
            IsLoaded = false;
            return Task.CompletedTask;
        }
        #endregion

        async void Initialize()
        {
            Logger.Debug("Initializing the Scheduler...");
            Scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await Scheduler.Start();
            InitializeTasks();

            Logger.Debug("The Scheduler has been successfully initilized");
        }

        async void InitializeTasks()
        {
            foreach (var task in DatabaseManager.ScheduledTasks)
                await AddToSchedule(task);   
        }

        void UpdateStatus(bool busy = false, string status = "")
        {
            Core.Dispatcher.Invoke(() =>
            {
                IsBusy = busy;
                Status = status;
                RaisePropertyChanged(nameof(IsBusy));
                RaisePropertyChanged(nameof(Status));
            });
        }

        #region Internals
        Task<IPresentable> Clone(IPresentable presentable, RepeatInterval interval, ref DateTime previousDue)
        {
            DateTime due = new DateTime();
            switch (interval.Repetition)
            {
                case Repetition.Hourly:
                    due = previousDue.AddHours(interval.Interval);
                    break;

                case Repetition.Daily:
                    due = previousDue.AddDays(interval.Interval);
                    break;

                case Repetition.Weekly:
                    List<CheckableWeekday> weekdays = new List<CheckableWeekday>();
                    for (int i = 0; i < interval.WeekDays.Count; i++)
                        if (interval.WeekDays[i].IsSelected)
                            weekdays.Add(interval.WeekDays[i]);

                    int dayIndex = interval.Index % weekdays.Count;
                    int index = weekdays.FindIndex(d => d.Index == dayIndex);

                    CheckableWeekday nextDay = null;
                    if (index < weekdays.Count - 1)
                        nextDay = weekdays[index + 1];
                    else nextDay = weekdays[0];

                    int nextDayIndex = dayIndex < nextDay.Index ? nextDay.Index : nextDay.Index + 7;

                    int diff = Math.Abs(dayIndex - nextDayIndex);
                    due = previousDue.AddDays(diff);

                    break;

                case Repetition.Monthly:
                    due = previousDue.AddMonths(interval.Interval);
                    break;

                case Repetition.Yearly:
                    due = previousDue.AddYears(interval.Interval);
                    break;
            }

            previousDue = due;
            return Task.FromResult(presentable.ReplicateDue(due));
        }

        IAsyncEnumerable<IPresentable> CloneForCalender(IPresentable task, RepeatInterval interval, bool addCurrentTask = true)
        {
            return AsyncEnum.Enumerate<IPresentable>(async consumer =>
            {
                DateTime expirationDate = interval.ExpirationDate;
                ExpirationType expirationType = interval.Expiration;

                if (!interval.IsFinite)
                {
                    expirationType = ExpirationType.Date;
                    expirationDate = new DateTime(Year.MaxYear, Month.MaxMonth, Day.MaxDay);
                }

                if (addCurrentTask)
                    await consumer.YieldAsync(task);

                IPresentable p = task;
                DateTime due = task.Due;
                switch (expirationType)
                {
                    case ExpirationType.Count:
                        for (int i = 0; i < interval.ExpirationCount; i++)
                        {
                            p = await Clone(task, interval, ref due);
                            await consumer.YieldAsync(p);
                        }
                        break;

                    case ExpirationType.Date:
                        while (p.Due < expirationDate)
                        {
                            p = await Clone(task, interval, ref due);
                            await consumer.YieldAsync(p);
                        }
                        break;
                }

                return;
            });
        }
        #endregion

        #endregion
    }
}
