using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Resources.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Month : TimeElement
    {
        #region Properties
        #region Statics
        public const int MinMonth = 1;
        public const int MaxMonth = 12;
        #endregion

        public ObservableCollection<Day> Days { get; } = new ObservableCollection<Day>();
        public Year Year { get; }

        public override int ElementCount => Tasks.Count;

        List<IPresentable> Tasks { get; } = new List<IPresentable>();

        #region Overrides
        public override bool IsActive => Year.IsActive && DateTime.Now.Month == this;

        public override DateTime Date => new DateTime(Year, this, Day.MinDay);

        public override ICollectionViewLiveShaping Elements => throw new NotImplementedException();
        #endregion

        #endregion

        #region Accessors
        public Day this[int i] => Days[i];
        #endregion

        #region Constructors
        public Month(Year year, int month) : base(month)
        {
            Year = year;
            Value = month;
        }
        #endregion

        #region Methods 

        public void AddDay(Day day)
        {
            Days.Add(day);
        }

        public override void Initialize()
        {
            for (int i = 0; i < Days.Count; i++)
                Days[i].Initialize();
        }

        #region Overrides
        public override string ToString() => ((StringCollection)Application.Current.Resources["MONTHS_OF_THE_YEAR"])[this-1];
        #endregion

        #endregion
    }
}
