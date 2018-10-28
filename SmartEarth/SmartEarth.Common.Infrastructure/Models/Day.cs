using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Resources.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Day : TimeElement
    {
        #region Properties

        #region Statics
        public const int MinDay = 1;
        public const int MaxDay = 31;
        #endregion

        Month _month; 
        public Month Month { get => _month; set => SetProperty(ref _month, value); }

        Year _year;
        public Year Year { get => _year; set => SetProperty(ref _year, value); }

        #region Overrides
        public override bool IsActive => Year.IsActive && Month.IsActive && DateTime.Now.Day == this;

        public AsyncSortableObservableCollection<IPresentable> Tasks { get; private set; } //= new AsyncObservableCollection<IPresentable>();

        public override int ElementCount => Tasks.Count;

        ICollectionViewLiveShaping _elements;
        public override ICollectionViewLiveShaping Elements => _elements;

        public override DateTime Date => new DateTime(Year, Month, this, 1, 0, 0);

        public override int Value
        {
            get => base.Value;
            protected set
            {
                if (value < MinDay) value = MinDay;
                else if (value > MaxDay) value = MaxDay;
                base.Value = value;
            }
        }

        public string Ordinal => ToOrdinal(this);
        #endregion

        public bool HoursActive => Hours.Count >= 0;

        public ObservableCollection<Hour> Hours { get; } = new ObservableCollection<Hour>();

        #region Presentation
        double _viewOffsetX = .0;
        public double ViewOffsetX
        {
            get => _viewOffsetX;
            set
            {
                if (value > ViewWidth)
                    value = ViewWidth;
                else if (value < 0) value = 0;
                SetProperty(ref _viewOffsetX, value);
            }
        }

        double _viewOffsetY = .0;
        public double ViewOffsetY
        {
            get => _viewOffsetY;
            set
            {
                if (value > ViewHeight)
                    value = ViewHeight;
                else if (value < 0) value = 0;
                SetProperty(ref _viewOffsetY, value);
            }
        }

        double _viewWidth = .0;
        double _viewHeight = .0;
        public double ViewWidth { get => _viewWidth; set => SetProperty(ref _viewWidth, value); }
        public double ViewHeight { get => _viewHeight; set => SetProperty(ref _viewHeight, value); }
        #endregion

        #endregion

        #region Accessors
        public Hour this[int i] => Hours[i];
        #endregion

        #region Constructors
        public Day(Month month, Year year, int value) : base(value)
        {
            Month = month;
            Year = year;
            Value = value;
        }
        #endregion

        #region Methods
        public void BuildHours()
        {
            Core.Dispatcher.Invoke(() => Hours.Clear());
            int index = -1;

            List<Hour> hours = new List<Hour>();
            foreach (var task in Tasks)
            {
                Hour h = new Hour(Year, Month, this, task.Due.Hour);
                if ((index = hours.IndexOf(h)) > -1) h = hours[index];
                else hours.Add(h);

                h.AddTask(task);
            }

            Core.Dispatcher.Invoke(() =>
            {
                Hours.AddRange(hours.OrderBy(h => h.Date));
                RaisePropertyChanged(nameof(Hours));
            });
        }

        public void DemolishHours() => Hours.Clear();

        public void AddTask(IPresentable task)
        {
            Tasks.Add(task);
            bool found = false;

            for (int i = 0; i < Hours.Count && !found; i++)
                if (found = (Hours[i] == task.Due.Hour))
                    Hours[i].AddTask(task);

            Application.Current.Dispatcher.Invoke(() =>
            {
                RaisePropertyChanged(nameof(Elements));
                RaisePropertyChanged(nameof(ElementCount));
            });
        }

        #region Overrides
        public override void Initialize()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Tasks = new AsyncSortableObservableCollection<IPresentable>();
                Tasks.SortingSelector = new Func<IPresentable, object>(p => p.Due);
                Tasks.Descending = true;
            });
        }
        public override string ToString() => ((StringCollection)Application.Current.Resources["DAYS_OF_THE_WEEK"])[(int)Date.DayOfWeek];
        #endregion



        #region Conversions
        public static implicit operator string(Day day)
        {
            if (day == null) return string.Empty;
            return day.ToString();
        }
        #endregion

        public static string ToOrdinal(int value)
        {
            string ordinal = "th";

            int lastDigits = value % 100;

            if (lastDigits < 11 || lastDigits > 13)
                switch (lastDigits % 10)
                {
                    case 1: ordinal = "st"; break;
                    case 2: ordinal = "nd"; break;
                    case 3: ordinal = "rd"; break;
                }
            return value.ToString("#,##0") + ordinal;
        }
        #endregion
    }
}
