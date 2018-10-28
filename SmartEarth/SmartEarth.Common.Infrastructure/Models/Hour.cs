using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Hour : TimeElement
    {
        #region Properties

        #region Statics
        public const int MinHour = 0;
        public const int MaxHour = 23;
        #endregion

        Year _year;
        public Year Year { get => _year; set => SetProperty(ref _year, value); }

        Month _month;
        public Month Month { get => _month; set => SetProperty(ref _month, value); }

        Day _day;
        public Day Day { get => _day; set => SetProperty(ref _day, value); }


        #region Overrides
        public override bool IsActive => Year.IsActive && Month.IsActive && Day.IsActive && DateTime.Now.Hour == this;

        public override DateTime Date => new DateTime(Year, Month, Day, this, 0, 0);

        ObservableCollection<IPresentable> Tasks { get; } = new ObservableCollection<IPresentable>();

        public override int ElementCount => Tasks.Count;

        public override ICollectionViewLiveShaping Elements { get; }

        public override int Value
        {
            get => base.Value;
            protected set
            {
                if (value < MinHour) value = MinHour;
                else if (value > MaxHour) value = MaxHour;
                base.Value = value;
            }
        }
        #endregion

        #endregion

        #region Constructors
        public Hour(Year year, Month month, Day day, int value) : base(value)
        {
            Elements = (ICollectionViewLiveShaping)CollectionViewSource.GetDefaultView(Tasks);
            Elements.IsLiveSorting = true;

            ((ICollectionView)Elements).SortDescriptions.Add(new SortDescription("Due", ListSortDirection.Ascending));


            Year = year;
            Month = month;
            Day = day; 
        }
        #endregion

        #region Methods
        public void AddTask(IPresentable task)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Tasks.Contains(task)) Tasks.Add(task);
                RaisePropertyChanged(nameof(Tasks));
            });
        }

        #region Overrides
        public override void Initialize()
        {
            throw new NotImplementedException();
        }
        public override string ToString() => Date.ToString("hh:mm tt");
        #endregion
        #endregion
    }
}
