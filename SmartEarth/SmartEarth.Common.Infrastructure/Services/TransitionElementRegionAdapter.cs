using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Transitionals.Controls;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class TransitionElementRegionAdapter : RegionAdapterBase<TransitionElement>
    {
        #region Constructors
        public TransitionElementRegionAdapter(IRegionBehaviorFactory factory) : base(factory) { }
        #endregion

        #region Methods

        #region Overrides
        /// <summary>
        /// Adapts a <see cref="TransitionElement"/> to an <see cref="IRegion"/>.
        /// </summary>
        /// <param name="region">The new region being used.</param>
        /// <param name="regionTarget">The object to adapt.</param>
        protected override void Adapt(IRegion region, TransitionElement regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));
            

            bool contentIsSet = regionTarget.Content != null;
            contentIsSet = contentIsSet || (BindingOperations.GetBinding(regionTarget, TransitionElement.ContentProperty) != null);

            if (contentIsSet)
                throw new InvalidOperationException("The Transition Element already contains content");

            region.ActiveViews.CollectionChanged += delegate
            {
                var view = region.ActiveViews.FirstOrDefault();

                // Don't use null views :)
                if (view != null) regionTarget.Content = view;
            };

            region.Views.CollectionChanged +=
                (sender, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add && region.ActiveViews.Count() == 0)
                    {
                        region.Activate(e.NewItems[0]);
                    }
                };
        }

        /// <summary>
        /// Creates a new instance of <see cref="SingleActiveRegion"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="SingleActiveRegion"/>.</returns>
        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }
        #endregion

        #endregion
    }
}
