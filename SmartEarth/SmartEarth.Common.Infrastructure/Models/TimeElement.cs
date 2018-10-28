using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartEarth.Common.Infrastructure.Models
{
    public abstract class TimeElement : BindableBase
    {
        #region Properties
        public abstract bool IsActive { get; }

        bool _isEnabled = true;
        public virtual bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }

        bool _isSelected = false;
        public virtual bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        int _value;
        public virtual int Value { get => _value; protected set => SetProperty(ref _value, value); }

        public abstract DateTime Date { get; }

        public abstract int ElementCount { get; }

        public abstract ICollectionViewLiveShaping Elements { get; }
        #endregion

        #region Constructors
        public TimeElement(int value) => Value = value;
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareTimeElements(TimeElement t1, TimeElement t2)
        {
            bool nullOne = ReferenceEquals(null, t1);
            bool nullTwo = ReferenceEquals(null, t2);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return t1.Date == t2.Date;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is TimeElement)) return false;
            return CompareTimeElements(this, (TimeElement)obj);
        }

        public static bool operator ==(TimeElement t1, TimeElement t2) => CompareTimeElements(t1, t2);
        public static bool operator !=(TimeElement t1, TimeElement t2) => !CompareTimeElements(t1, t2);


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Date.GetHashCode();
                hash = hash * 23 + Value.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region Convertions
        public static implicit operator int(TimeElement element)
        {
            if (element == null) return 0;
            return element.Value;
        }
        #endregion

        public abstract void Initialize();
        #endregion
    }
}
