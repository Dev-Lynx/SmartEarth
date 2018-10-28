using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Resources.Collections
{
    public class AsyncSuppressedObservableCollection<T> : AsyncObservableCollection<T>
    {
        #region Properties
        public bool SuppressNotification { get; set; } = false;
        #endregion

        #region Constructors
        public AsyncSuppressedObservableCollection() { }
        public AsyncSuppressedObservableCollection(IEnumerable<T> list) : base(list) { }
        #endregion

        #region Methods

        public virtual void AddRange(IEnumerable<T> list, bool notify = true)
        {
            if (list == null)
                throw new ArgumentException("The list cannot be null");
            SuppressNotification = true;

            int index = Count;
            foreach (T item in list) Add(item);
            SuppressNotification = !notify;

            //foreach (T item in list)
              //  OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index++));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public async Task AddRangeAsync(IEnumerable<T> list, bool notify = true)
        {
            if (list == null)
                throw new ArgumentException("The list cannot be null");
            SuppressNotification = true;

            int index = Count;
            bool complete = false;

            while (!complete)
            {
                try
                {
                    foreach (T item in list) Add(item);
                    complete = true;
                }
                catch { await Task.Delay(100); }
            }

            
            SuppressNotification = !notify;

            //foreach (T item in list)
            //  OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index++));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        #region Overrides
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressNotification) base.OnCollectionChanged(e);
        }
        #endregion

        #endregion

    }
}
