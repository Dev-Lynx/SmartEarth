using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class Year : TimeElement
    {
        #region Properties

        #region Statics
        public const int MinYear = 2000;
        public const int MaxYear = 2099;
        #endregion

        #region Overrides
        public override bool IsActive => DateTime.Now.Year == this;

        public override DateTime Date => new DateTime(this, Month.MinMonth, Day.MinDay);

        public override int ElementCount => Tasks.Count;

        List<IPresentable> Tasks { get; } = new List<IPresentable>();

        public override ICollectionViewLiveShaping Elements => throw new NotImplementedException();
        #endregion


        public ObservableCollection<Month> Months { get; } = new ObservableCollection<Month>();
        #endregion

        #region Constructors
        Year(int year) : base(year)
        {

        }
        #endregion

        #region Accessors
        public Month this[int i] => Months[i];
        #endregion

        #region Methods
        public static Task<Year> GenerateYear(int year, bool forceLimit = false)
        {
            if (year > DateTime.MaxValue.Year && !forceLimit) year = DateTime.MaxValue.Year;
            else if (year < DateTime.MinValue.Year && !forceLimit) year = DateTime.MinValue.Year;

            var date = new DateTime(year, 1, 1);

            var y = new Year(year);
            for (int i = 1; i < Month.MaxMonth+1; i++)
            {
                var month = new Month(y, i);
                var daysInMonth = DateTime.DaysInMonth(year, i);
                for (int j = 1; j < daysInMonth+1; j++)
                    month.AddDay(new Day(month, y, j));
                y.Months.Add(month);
            }
            return Task.FromResult<Year>(y);
        }

        public override void Initialize()
        {
            for (int i = 0; i < Months.Count; i++)
                Months[i].Initialize();
        }

        #region Equity 
        static bool CompareYears(Year y1, Year y2)
        {
            bool nullOne = ReferenceEquals(y1, null);
            bool nullTwo = ReferenceEquals(y2, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return y1.Value == y2.Value;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Year)) return false;
            return CompareYears(this, (Year)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash + 23 * Value.GetHashCode();
                hash = hash + 23 * Months.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"{Value}";
        #endregion

        #endregion
    }
}
