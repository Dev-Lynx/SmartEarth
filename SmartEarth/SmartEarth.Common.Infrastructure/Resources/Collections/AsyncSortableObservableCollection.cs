using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Resources.Collections
{
    public class AsyncSortableObservableCollection<T> : AsyncObservableCollection<T>
    {
        #region Properties
        public Func<T, object> SortingSelector { get; set; }
        public bool Descending { get; set; }
        #endregion

        #region Constructors
        public AsyncSortableObservableCollection() { }
        public AsyncSortableObservableCollection(IEnumerable<T> list) : base(list) { }
        #endregion

        #region Methods

        #region Overrides
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset || e.Action == NotifyCollectionChangedAction.Move || SortingSelector == null)
            {
                base.OnCollectionChanged(e);
                return;
            }

            var query = this.Select((item, index) => (Item: item, Index: index));
            query = Descending ? query.OrderBy(tuple => SortingSelector(tuple.Item))
                : query.OrderByDescending(tuple => SortingSelector(tuple.Item));

            var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index))
             .Where(o => o.OldIndex != o.NewIndex);

            using (var enumerator = map.GetEnumerator())
                if (enumerator.MoveNext())
                    Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);

            base.OnCollectionChanged(e);
        }
        #endregion

        #endregion
    }
}
