using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Resources.Collections
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        #region Properties
        SynchronizationContext Context { get; } = SynchronizationContext.Current;
        #endregion

        #region Constructors
        public AsyncObservableCollection() { }
        public AsyncObservableCollection(IEnumerable<T> list) : base(list) { }
        #endregion

        #region Methods

        #region Overrides
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Context == SynchronizationContext.Current) RaiseCollectionChanged(e);
            else Context.Send(RaiseCollectionChanged, e);
        }

        private void RaiseCollectionChanged(object param) => base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (Context == SynchronizationContext.Current) RaisePropertyChanged(e);
            else Context.Send(RaisePropertyChanged, e);
        }

        private void RaisePropertyChanged(object param) => base.OnPropertyChanged((PropertyChangedEventArgs)param);
        #endregion

        #endregion
    }
}
