using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Resources.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class RepeatInterval : BindableBase
    {
        #region Properties

        #region Statics
        const int MinHours = 1;
        const int MaxHours = 1000;
        const int MinDays = 1;
        const int MaxDays = 365;
        const int MinWeeks = 1;
        const int MaxWeeks = 200;
        const int MinMonths = 1;
        const int MaxMonths = 100;
        const int MinYears = 1;
        const int MaxYears = 10;
        const int MinExpiration = 2;
        const int MaxExpiration = 1000;
        static readonly List<string> RepetitionTexts = new List<string>(((StringCollection)Application.Current.Resources["days_weeks_months_years"]));

        
        #endregion

        public ObservableCollection<CheckableWeekday> WeekDays { get; set; } = new ObservableCollection<CheckableWeekday>();

        bool _isEnabled;
        public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }

        public int Index { get; set; } = 0;

        Repetition _repetition;
        public Repetition Repetition
        {
            get => _repetition;
            set
            {
                SetProperty(ref _repetition, value);
                Reload();
                RaisePropertyChanged(nameof(IsWeekly));
                RaisePropertyChanged(nameof(RepetitionText));
                RaisePropertyChanged(nameof(WeekDays));
            }
        }

        public string RepetitionText
        {
            get => RepetitionTexts[(int)Repetition];
        }

        bool _isFinite = false;
        public bool IsFinite
        {
            get => _isFinite;
            set
            {
                SetProperty(ref _isFinite, value);
                RaisePropertyChanged(nameof(Expiration));
                RaisePropertyChanged(nameof(ExpiresOnDate));
                RaisePropertyChanged(nameof(ExpiresOnCount));
            }

        }

        ExpirationType _expiration;
        public ExpirationType Expiration
        {
            get => _expiration;
            set
            {
                SetProperty(ref _expiration, value);
                RaisePropertyChanged(nameof(ExpiresOnDate));
                RaisePropertyChanged(nameof(ExpiresOnCount));
            }
        }

        int _expirationCount = 1;
        public int ExpirationCount
        {
            get => _expirationCount;
            set
            {
                if (value < MinExpiration) value = MinExpiration;
                else if (value > MaxExpiration) value = MaxExpiration;
                SetProperty(ref _expirationCount, value);
            }
        }

        DateTime _expirationDate = DateTime.Now.AddMonths(3);
        public DateTime ExpirationDate { get => _expirationDate; set => SetProperty(ref _expirationDate, value); }

        public bool ExpiresOnDate => IsEnabled && IsFinite && Expiration == ExpirationType.Date;
        public bool ExpiresOnCount => IsEnabled && IsFinite && Expiration == ExpirationType.Count;

        int _interval = MinDays;
        public int Interval
        {
            get => _interval;
            set
            {
                // Compare and keep the repeat interval within the limits...
                switch (Repetition)
                {
                    case Repetition.Hourly:
                        if (value < MinHours) value = MinHours;
                        else if (value > MaxHours) value = MaxHours;
                        break;

                    case Repetition.Daily:
                        if (value < MinDays) value = MinDays;
                        else if (value > MaxDays) value = MaxDays;
                        break;

                    case Repetition.Weekly:
                        if (value < MinWeeks) value = MinWeeks;
                        else if (value > MaxWeeks) value = MaxWeeks;
                        break;

                    case Repetition.Monthly:
                        if (value < MinMonths) value = MinMonths;
                        else if (value > MaxMonths) value = MaxMonths;
                        break;

                    case Repetition.Yearly:
                        if (value < MinYears) value = MinYears;
                        else if (value > MaxYears) value = MaxYears;
                        break;
                }

                SetProperty(ref _interval, value);
            }
        }

        public bool IsWeekly => Repetition == Repetition.Weekly;
        #endregion

        #region Constructors
        public RepeatInterval() { Reload(); }
        #endregion

        #region Methods
        void Reload()
        {
            switch (Repetition)
            {
                case Repetition.Hourly:
                case Repetition.Daily:
                case Repetition.Monthly:
                case Repetition.Yearly:
                    WeekDays.Clear();
                    break;

                case Repetition.Weekly:
                    if (WeekDays.Count > 0) break; 
                    var week = (StringCollection)Application.Current.Resources["DAYS_OF_THE_WEEK_SHORT"];
                    for (int i = 0; i < week.Count; i++) WeekDays.Add(new CheckableWeekday(week[i], i + 1));
                    break;
            }

        }

        public static RepeatInterval CloneNext(RepeatInterval interval)
        {
            RepeatInterval next = new RepeatInterval()
            {
                IsEnabled = interval.IsEnabled,
                Index = interval.Index+1,
                Repetition = interval.Repetition,
                IsFinite = interval.IsFinite,
                Expiration = interval.Expiration, 
                ExpirationCount = interval.ExpirationCount,
                ExpirationDate = interval.ExpirationDate,
                Interval = interval.Interval
            };

            for (int i = 0; i < interval.WeekDays.Count; i++)
                next.WeekDays.Add(interval.WeekDays[i]);

            return next;
        }
        #endregion
    }
}
