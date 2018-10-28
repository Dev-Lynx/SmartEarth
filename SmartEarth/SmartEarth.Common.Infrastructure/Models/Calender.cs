using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Resources.Collections;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Calender : BindableBase, IEnumerable<Year>, ICalender
    {
        #region Properties

        #region Statics
        const int ViewSize = 42;
        const int NavigateAnimationDuration = 400;

        #region Collections
        public static ObservableCollection<string> DaysOfTheWeek { get; } = (ObservableCollection<string>)Application.Current.Resources["DAYS_OF_THE_WEEK"];
        public static ObservableCollection<string> MonthsOfTheYear { get; } = (ObservableCollection<string>)Application.Current.Resources["MONTHS_OF_THE_YEAR"];
        #endregion

        #endregion

        public int StartYear { get; private set; }
        public int EndYear { get; private set; }
        public List<IPresentable> Tasks { get; } = new List<IPresentable>();

        #region Events
        public event EventHandler DateChanged;
        #endregion

        public CalenderView Context { get; private set; } = CalenderView.Month;

        NavigateDirection _navigateDirection;
        public NavigateDirection NavigateDirection
        {
            get => _navigateDirection;
            private set
            {

                _navigateDirection = value;
                RaisePropertyChanged(nameof(NavigateDirection));
            }
        }

        #region Views
        public AsyncSuppressedObservableCollection<Year> Years { get; private set; }
        public AsyncSuppressedObservableCollection<TimeElement> PreviousView { get; private set; }
        public AsyncSuppressedObservableCollection<TimeElement> CurrentView { get; private set; }
        public AsyncSuppressedObservableCollection<TimeElement> NextView { get; private set; }
        public AsyncSuppressedObservableCollection<TimeElement> DummyView { get; private set; }
        #endregion

        #region Time
        Day _selectedDay = null;
        public Day SelectedDay
        {
            get
            {
                if (_selectedDay == null)
                {
                    SelectedDay = CurrentDay;
                    return SelectedDay;
                }
                return _selectedDay;
            }
            set
            {
                if (value != null && (value.Year < StartYear || value.Year > EndYear)) return;
                if (_selectedDay != null) _selectedDay.IsSelected = false;
                if (value != null) value.IsSelected = true;
                SetProperty(ref _selectedDay, value);
                NotifyChange(IsBusy, ViewIsLoaded, NavigateDirection);
            }
        }

        Day _currentDay;
        public Day CurrentDay
        {
            get => _currentDay;
            private set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (value != null && (value.Year < StartYear || value.Year > EndYear)) return;
                    SetProperty(ref _currentDay, value);
                    
                    RaisePropertyChanged(nameof(CurrentYear));
                    RaisePropertyChanged(nameof(CurrentMonth));
                    RaisePropertyChanged(nameof(CurrentDay));
                });

                
            }
        }

        public Year CurrentYear => CurrentDay.Year;

        public Month CurrentMonth => CurrentDay.Month;

        #endregion

        #region Flags
        bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                bool wasBusy = _isBusy;
                SetProperty(ref _isBusy, value);
            }
        }

        bool _viewIsLoaded = false;
        public bool ViewIsLoaded { get => _viewIsLoaded; set => SetProperty(ref _viewIsLoaded, value); }

        public bool CanPrevious => (CurrentMonth > Month.MinMonth || CurrentYear > StartYear) && !IsBusy;
        public bool CanNext => (CurrentMonth < Month.MaxMonth || CurrentYear < EndYear) && !IsBusy;
        #endregion

        #region Internals
        CancellationTokenSource CancellationSource { get; set; } = new CancellationTokenSource();
        bool ViewIsToday => CurrentYear == DateTime.Now.Year && CurrentMonth == DateTime.Now.Month;
        bool Waiting { get; set; }

        List<Year> Eras { get; } = new List<Year>();
        #endregion

        #endregion

        #region Constructors
        Calender() : this(Year.MinYear, Year.MaxYear) { }
        Calender(int startYear, int endYear)
        {
            StartYear = startYear;
            EndYear = endYear;
            Application.Current.Dispatcher.Invoke(() =>
            {
                DummyView = new AsyncSuppressedObservableCollection<TimeElement>();
                DummyView.SuppressNotification = true;
                for (int i = 0; i < ViewSize; i++)
                    DummyView.Add(null);
                DummyView.SuppressNotification = false;
                DummyView.Reset();

                Years = new AsyncSuppressedObservableCollection<Year>();
                RaisePropertyChanged(nameof(DummyView));
            });
        }
        #endregion

        #region Methods

        #region IEnumerable Implementation
        public IEnumerator<Year> GetEnumerator() => Years.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Years.GetEnumerator();
        #endregion

        #region Statics
        public static async Task<Calender> Build(int startYear, int endYear)
        {
            Calender calender = new Calender(startYear, endYear);
            await calender.Build();
            return calender;
        }
        #endregion

        #region Controls
        public void Initialize()
        {
            IsBusy = true;
            Years.SuppressNotification = true;
            for (int i = 0; i < Eras.Count; i++)
            {
                var era = Eras[i];
                era.Initialize();

                if (era >= StartYear && era <= EndYear)
                    Years.Add(era);
            }
            Years.SuppressNotification = false;
            Years.Reset();
            RaisePropertyChanged(nameof(Years));
            IsBusy = false;
        }

        public async Task Previous(bool delay = true)
        {
            if (!(CurrentMonth > Month.MinMonth || CurrentYear > StartYear)) return;
            if (Waiting) return;

            while (IsBusy && delay)
            {
                await Task.Delay(10);
                Waiting = true;
            }
            Waiting = false;
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
                ViewIsLoaded = false;

                NavigateDirection = NavigateDirection.Left; 
            });

            await Task.Delay(NavigateAnimationDuration);
            await Adapt();
            await LoadViews();

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
                RaisePropertyChanged(nameof(SelectedDay));
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
                NavigateDirection = NavigateDirection.None;
            });
        }

        public async Task Next(bool delay = true)
        {
            if (!(CurrentMonth < Month.MaxMonth || CurrentYear < EndYear)) return;
            if (Waiting) return;

            while (IsBusy && delay)
            {
                await Task.Delay(10);
                Waiting = true;
            }
            Waiting = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
                ViewIsLoaded = false;
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
                NavigateDirection = NavigateDirection.Right;
            });

            await Task.Delay(NavigateAnimationDuration);
            await Adapt();
            NavigateDirection = NavigateDirection.Right;
            await LoadViews();

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
                RaisePropertyChanged(nameof(SelectedDay));
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
                NavigateDirection = NavigateDirection.None;
            });
        }

        public void SetDate(DateTime date)
        {
            if (!(date.Year <= StartYear || date.Year >= EndYear))
                return;

            var index = date.Year - StartYear - 1;
            CurrentDay = Eras[index][date.Month - 1][date.Day - 1];
            if (ViewIsToday) CurrentDay = CurrentMonth[DateTime.Now.Day - 1];

            Application.Current.Dispatcher.Invoke(() =>
            {
                RaisePropertyChanged(nameof(CurrentDay));
                RaisePropertyChanged(nameof(CurrentMonth));
                RaisePropertyChanged(nameof(CurrentYear));
            });
        }

        public async void SetDate(TimeElement element)
        {
            if (Waiting) return;

            while (IsBusy)
            {
                await Task.Delay(10);
                Waiting = true;
            }
            Waiting = false;
            NavigateDirection direction = NavigateDirection.None;
            if (element is Day)
            {
                Day day = (Day)element;
                if (day == CurrentDay) return;
                var index = day.Year - StartYear;
                SelectedDay = Years[index][day.Month - 1][day - 1];
            }
            else if (element is Month)
            {
                Month month = (Month)element;
                if (month == CurrentMonth) return;
                var index = month.Year - StartYear;
                int diff = month - CurrentMonth;
                if (diff == -1) await Previous(false);
                else if (diff <= -2) direction = NavigateDirection.FarLeft;
                else if (diff == 1) await Next(false);
                else if (diff >= 2) direction = NavigateDirection.FarRight;

                CurrentDay = Years[index][month - 1][0];

                if (ViewIsToday) SelectedDay = CurrentMonth[DateTime.Now.Day - 1];
                else SelectedDay = null;
            }
            else if (element is Year)
            {
                Year year = (Year)element;
                if (year == CurrentYear) return;

                int diff = year - CurrentYear;
                if (diff < 0) direction = NavigateDirection.FarLeft;
                else if (diff > 0) direction = NavigateDirection.FarRight;

                CurrentDay = Years[(Year)element - StartYear][0][0];

                if (ViewIsToday) SelectedDay = CurrentMonth[DateTime.Now.Day - 1];
                else SelectedDay = null;
            }

            if (direction == NavigateDirection.None)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Waiting = false;
                    IsBusy = false;
                    ViewIsLoaded = true;
                    NavigateDirection = NavigateDirection.None;
                });
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
                ViewIsLoaded = false;
                NavigateDirection = direction;
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
            });

            await Task.Delay(NavigateAnimationDuration);
            await LoadViews();

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
                SelectedDay = null;
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
                NavigateDirection = NavigateDirection.None;
            });
        }
        #endregion

        #region Internals
        async Task Build()
        {
            Application.Current.Dispatcher.Invoke(() => IsBusy = true);
            Eras.Clear();
            var date = DateTime.Now;
            for (int year = StartYear-1; year <= EndYear+1; year++)
            {
                var y = await Year.GenerateYear(year);
                Eras.Add(y);

                if (date.Year == y.Value)
                    CurrentDay = y[date.Month - 1][date.Day - 1];
            }

            if (ViewIsToday) SelectedDay = CurrentMonth[DateTime.Now.Day - 1];
            else SelectedDay = null;

            await LoadViews();
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
                RaisePropertyChanged(nameof(Years));
                RaisePropertyChanged(nameof(IsBusy));
            });
        }

        Task Adapt()
        {
            switch (NavigateDirection)
            {
                case NavigateDirection.Left:
                    switch (Context)
                    {
                        case CalenderView.Month:
                            Month currentMonth = null;
                            if (CurrentMonth > Month.MinMonth) currentMonth = CurrentYear[CurrentMonth - 2];
                            else if (CurrentYear > StartYear) currentMonth = Eras[Eras.IndexOf(CurrentYear) - 1][Month.MaxMonth - 1];

                            CurrentDay = currentMonth[0];
                            
                            if (ViewIsToday) SelectedDay = currentMonth[DateTime.Now.Day - 1];
                            else SelectedDay = null;
                            break;
                    }
                    break;

                case NavigateDirection.Right:
                    switch (Context)
                    {
                        case CalenderView.Month:
                            Month currentMonth = null;
                            if (CurrentMonth < Month.MaxMonth) currentMonth = CurrentYear[CurrentMonth - 1 + 1];
                            else if (CurrentYear < EndYear) currentMonth = Eras[Eras.IndexOf(CurrentYear) + 1][Month.MinMonth - 1];

                            CurrentDay = currentMonth[0];
                            if (ViewIsToday) SelectedDay = currentMonth[DateTime.Now.Day - 1];
                            else SelectedDay = null;
                            break;
                    }
                    break;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewIsLoaded = false;
                RaisePropertyChanged(nameof(SelectedDay));
                RaisePropertyChanged(nameof(CurrentDay));
                RaisePropertyChanged(nameof(CurrentMonth));
                RaisePropertyChanged(nameof(CurrentYear));
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
            });

            return Task.CompletedTask;
        }

        async Task LoadViews()
        {
            switch (NavigateDirection)
            {
                case NavigateDirection.Left:
                    CurrentView.SuppressNotification = true;
                    NextView.SuppressNotification = true;

                    NextView.Clear();
                    await NextView.AddRangeAsync(CurrentView, false);
                    CurrentView.Clear();
                    CurrentView.AddRange(PreviousView, true);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < CurrentView.Count; i++)
                        {
                            var element = (Day)CurrentView[i];
                            element.IsEnabled = element.Month == CurrentDay.Month;
                        }

                        RaisePropertyChanged(nameof(CurrentView));
                        ViewIsLoaded = true;
                    });

                    await Task.Delay(100);
                    NextView.Reset();

                    await Task.Run(async () =>
                    {
                        PreviousView = await Generate(await GetPreviousMonth());
                        Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(PreviousView)));
                    });
                    break;

                case NavigateDirection.Right:
                    CurrentView.SuppressNotification = true;
                    PreviousView.SuppressNotification = true;


                    PreviousView.Clear();
                    await PreviousView.AddRangeAsync(CurrentView);
                    CurrentView.Clear();
                    CurrentView.AddRange(NextView, true);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < CurrentView.Count; i++)
                        {
                            var element = (Day)CurrentView[i];
                            element.IsEnabled = element.Month == CurrentDay.Month;
                        }

                        RaisePropertyChanged(nameof(CurrentView));
                        ViewIsLoaded = true;
                    });

                    await Task.Delay(100);
                    PreviousView.Reset();

                    await Task.Run(async () =>
                    {
                        NextView = await Generate(await GoForwardByMonth(CurrentMonth, 1));
                        Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(NextView)));
                    });

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RaisePropertyChanged(nameof(NextView));
                    });
                    break;

                case NavigateDirection.None:
                case NavigateDirection.FarLeft:
                case NavigateDirection.FarRight:
                    CurrentView = await Generate(CurrentMonth);
                    
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        for (int i = 0; i < CurrentView.Count; i++)
                        {
                            var element = (Day)CurrentView[i];
                            element.IsEnabled = element.Month == CurrentDay.Month;
                        }

                        RaisePropertyChanged(nameof(CurrentView));
                        ViewIsLoaded = true;
                    });

                    await Task.Run(async () =>
                    {
                        NextView = await Generate(await GetNextMonth(CurrentMonth));
                        PreviousView = await Generate(await GetPreviousMonth(CurrentMonth));
                    });
                    
                    Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(PreviousView)));
                    Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(CurrentView)));
                    Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(NextView)));
                    break;
            }

        }

        #region Navigations
        Task<Month> GetPreviousMonth(Month month = null)
        {
            if (month == null) month = CurrentMonth;
            if (month.Value > Month.MinMonth) return Task.FromResult(month.Year[month - 2]);
            else if (month.Year > StartYear)
                return Task.FromResult(Eras[Eras.IndexOf(month.Year) - 1][Month.MaxMonth - 1]);

            return Task.FromResult(Eras[0][Month.MaxMonth - 1]);
        }

        async Task<Month> GoBackwardsByMonth(Month month, int depth)
        {
            if (depth-- <= 0) return month;
            return await GoBackwardsByMonth(await GetPreviousMonth(month), depth);
        }

        async Task<Month> GoForwardByMonth(Month month, int depth)
        {
            if (depth-- <= 0) return month;
            return await GoForwardByMonth(await GetNextMonth(month), depth);
        }

        Task<Month> GetNextMonth(Month month = null)
        {
            if (month == null) month = CurrentMonth;
            if (month < Month.MaxMonth) return Task.FromResult(month.Year[month.Value - 1 + 1]);
            else if (month.Year < EndYear) return Task.FromResult(Eras[Eras.IndexOf(month.Year) + 1][Month.MinMonth - 1]);

            return Task.FromResult(Eras[Eras.Count - 1][Month.MaxMonth - 1]);
        }
        #endregion

        #region Generations
        async Task<List<TimeElement>> GenerateList(Month month)
        {
            if (month == null) month = CurrentMonth;
            var collection = new List<TimeElement>();

            int day = (int)month.Days[0].Date.DayOfWeek;
            var previous = await GetPreviousMonth(month);

            if (CancellationSource.IsCancellationRequested) return collection;

            // Add a fragment of the previous month if necessary
            for (int i = day - 1; i >= 0; i--)
            {
                var d = previous.Days[previous.Days.Count - 1 - i];
                collection.Add(d);
                d.IsEnabled = false;
                await Task.Delay(10);
                if (CancellationSource.IsCancellationRequested) return collection;
            }


            for (int i = 0; i < month.Days.Count; i++)
            {
                var d = month.Days[i];
                collection.Add(d);
                d.IsEnabled = true;
                await Task.Delay(10);
                if (CancellationSource.IsCancellationRequested) return collection;
            }

            var remainder = ViewSize - (day + month.Days.Count);
            var next = await GetNextMonth(month);

            for (int i = 0; i < remainder; i++)
            {
                var d = next.Days[i];
                collection.Add(d);
                d.IsEnabled = false;
                await Task.Delay(10);
                if (CancellationSource.IsCancellationRequested) return collection;
            }

            return collection;
        }

        async Task<AsyncSuppressedObservableCollection<TimeElement>> Generate(Month month)
        {
            if (month == null) month = CurrentMonth;
            var collection = await Application.Current.Dispatcher.InvokeAsync(() => new AsyncSuppressedObservableCollection<TimeElement>());
            collection.SuppressNotification = true;
            collection.Clear();

            int day = (int)month.Days[0].Date.DayOfWeek;
            var previous = await GetPreviousMonth(month);

            // Add a fragment of the previous month if necessary
            for (int i = day - 1; i >= 0; i--)
            {
                var d = previous.Days[previous.Days.Count - 1 - i];
                collection.Add(d);
                await Task.Delay(10);
            }


            for (int i = 0; i < month.Days.Count; i++)
            {
                var d = month.Days[i];
                collection.Add(d);
                await Task.Delay(10);
            }

            var remainder = ViewSize - (day + month.Days.Count);
            var next = await GetNextMonth(month);

            for (int i = 0; i < remainder; i++)
            {
                var d = next.Days[i];
                collection.Add(d);
                await Task.Delay(10);
            }

            collection.SuppressNotification = false;
            return collection;
        }

        async Task<TimeElementCollection> GenerateCollection(Month month)
        {
            if (month == null) month = CurrentMonth;
            var collection = await Application.Current.Dispatcher.InvokeAsync(() => new TimeElementCollection(this));
            collection.SuppressNotification = true;
            collection.Clear();

            int day = (int)month.Days[0].Date.DayOfWeek;
            var previous = await GetPreviousMonth(month);

            // Add a fragment of the previous month if necessary
            for (int i = day - 1; i >= 0; i--)
            {
                var d = previous.Days[previous.Days.Count - 1 - i];
                collection.Add(d);
                await Task.Delay(10);
            }


            for (int i = 0; i < month.Days.Count; i++)
            {
                var d = month.Days[i];
                collection.Add(d);
                await Task.Delay(10);
            }

            var remainder = ViewSize - (day + month.Days.Count);
            var next = await GetNextMonth(month);

            for (int i = 0; i < remainder; i++)
            {
                var d = next.Days[i];
                collection.Add(d);
                await Task.Delay(10);
            }

            collection.SuppressNotification = false;
            return collection;
        }

        async Task EnableElements(IEnumerable<TimeElement> elements)
        {
            bool enabled = false;
            foreach (var element in elements)
                switch (Context)
                {
                    case CalenderView.Month:
                        enabled = element.Date.Month == CurrentMonth;
                        await Application.Current.Dispatcher.InvokeAsync(() => element.IsEnabled = enabled);
                        break;
                }
        }

        async Task EnableElements(IEnumerable<TimeElement> elements, ObservableCollection<TimeElement> container)
        {
            bool enabled = false;
            foreach (var element in elements)
                switch (Context)
                {
                    case CalenderView.Month:
                        enabled = element.Date.Month == CurrentMonth;
                        await Application.Current.Dispatcher.InvokeAsync(() => element.IsEnabled = enabled);
                        container.Add(element);
                        break;
                }
        }
        #endregion

        #region Syncronizations
        public async Task SynchronizeTasks(IEnumerable<IPresentable> tasks)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var task in tasks)
                {
                    if (!Tasks.Contains(task)) Tasks.Add(task);

                    // Find the right day for the task and attach it...
                    bool found = false;
                    for (int i = 0; i < Eras.Count && !found; i++)
                        if (found = (Eras[i].Value == task.Due.Year))
                            for (int j = 0; j < Month.MaxMonth && found; j++)
                                if (!(found = !(Eras[i][j] == task.Due.Month)))
                                    for (int k = 0; k < Eras[i][j].Days.Count && !found; k++)
                                        if (found = (Eras[i][j][k] == task.Due.Day))
                                            Eras[i][j][k].AddTask(task);
                }
            });
        }

        public Task SynchronizeTasks(params IPresentable[] tasks)
        {
            foreach (var task in tasks)
            {
                if (!Tasks.Contains(task)) Tasks.Add(task);

                // Find the right day for the task and attach it...
                bool found = false;
                for (int i = 0; i < Eras.Count && !found; i++)
                    if (found = (Eras[i].Value == task.Due.Year))
                        for (int j = 0; j < Month.MaxMonth && found; j++)
                            if (!(found = !(Eras[i][j] == task.Due.Month)))
                                for (int k = 0; k < Eras[i][j].Days.Count && !found; k++)
                                    if (found = (Eras[i][j][k] == task.Due.Day))
                                        Eras[i][j][k].AddTask(task);
            }
            return Task.CompletedTask;
        }
        #endregion

        void NotifyChange(bool busy = true, bool viewLoaded = false, NavigateDirection navigateDirection = NavigateDirection.None)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = busy;
                ViewIsLoaded = viewLoaded;
                NavigateDirection = navigateDirection;
                RaisePropertyChanged(nameof(SelectedDay));
                RaisePropertyChanged(nameof(CanPrevious));
                RaisePropertyChanged(nameof(CanNext));
                RaisePropertyChanged(nameof(CurrentDay));
                RaisePropertyChanged(nameof(CurrentMonth));
                RaisePropertyChanged(nameof(CurrentYear));
                DateChanged?.Invoke(this, EventArgs.Empty);
            });
        }
        #endregion

        #endregion
    }
}
