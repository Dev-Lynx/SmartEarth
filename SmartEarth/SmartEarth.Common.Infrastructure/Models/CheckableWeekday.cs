using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Resources.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class CheckableWeekday : BindableBase
    {
        #region Properties
        string _day = string.Empty;
        public string Day { get => _day; set => SetProperty(ref _day, value); }

        int _index;
        public int Index { get => _index; set => SetProperty(ref _index, value); }

        bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        #endregion

        #region Constructors
        public CheckableWeekday() { }
        public CheckableWeekday(string day, int index)
        {
            Day = day;
            Index = index;
        }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareWeekDays(CheckableWeekday w1, CheckableWeekday w2)
        {
            bool nullOne = ReferenceEquals(null, w1);
            bool nullTwo = ReferenceEquals(null, w2);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return w1.Index == w2.Index;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is CheckableWeekday)) return false;

            return CompareWeekDays(this, (CheckableWeekday)obj);
        }

        public static bool operator ==(CheckableWeekday c1, CheckableWeekday c2) => CompareWeekDays(c1, c2);
        public static bool operator !=(CheckableWeekday c1, CheckableWeekday c2) => !CompareWeekDays(c1, c2);

        public override int GetHashCode() => Index.GetHashCode();

        public override string ToString() => Day;
        #endregion

        #region Conversions
        public static implicit operator string (CheckableWeekday weekday)
        {
            if (weekday == null) return ((StringCollection)Application.Current.Resources["DAYS_OF_THE_WEEK"])[0];
            return weekday.Day;
        }
        #endregion

        #endregion
    }
}
