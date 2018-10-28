using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Resources.Collections
{
    public class TimeElementCollection : AsyncSuppressedObservableCollection<TimeElement>
    {
        #region Properties

        public CalenderView Context { get; set; } = CalenderView.Month;

        #region Internals
        ICalender Calender { get; } 
        #endregion

        #endregion

        #region Constructors
        public TimeElementCollection(ICalender calender)
        {
            Calender = calender;
        }
        #endregion

        #region Methods
        public override void AddRange(IEnumerable<TimeElement> list, bool notify = true)
        {
            if (list == null)
                throw new ArgumentException("The list cannot be null");
            SuppressNotification = true;

            foreach (TimeElement element in list)
            {
                switch (Calender.Context)
                {
                    case CalenderView.Month:
                        element.IsEnabled = element.Date.Month == Calender.CurrentDay.Month;
                        //Application.Current.Dispatcher.Invoke(() => element.IsEnabled = element.Date.Month == Calender.CurrentDay.Month);
                        break;
                }
                Add(element);
            }
            
            SuppressNotification = !notify;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion
    }
}
