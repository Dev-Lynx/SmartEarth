using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Resources.Collections
{
    /// <summary>
    /// Delays CollectionChanged Notification when a range of
    /// items are added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SuppressedObservableCollection<T> : ObservableCollection<T>
    {
        #region Properties
        bool SuppressNotification { get; set; } = false;
        #endregion

        #region Constructors
        public SuppressedObservableCollection() { }
        public SuppressedObservableCollection(IEnumerable<T> list) : base(list) { }
        #endregion

        #region Methods

        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentException("The list cannot be null");
            SuppressNotification = true;

            foreach (T item in list) Add(item);
            SuppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #region Overrides
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressNotification)
                base.OnCollectionChanged(e);
        }
        #endregion

        #endregion
    }
}
