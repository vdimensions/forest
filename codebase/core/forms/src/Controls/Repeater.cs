using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axle.Verification;

namespace Forest.Forms.Controls
{
    public static class Repeater
    {
        internal static class Regions
        {
            public const string Items = "Items";
        }

        /// <summary>
        /// An abstract class to serve as a base for views which will hold a number of identical repeating child views.
        /// </summary>
        /// <typeparam name="TItemView">The type of the repeater's items view.</typeparam>
        /// <typeparam name="TItemModel">The type of the repeater's items view model.</typeparam>
        public class View<TModel, TItemView, TItemModel> : LogicalView<TModel> where TItemView: IView<TItemModel>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly string _itemsRegionName;

            protected View(TModel model, string itemsRegionName) : base(model)
            {
                _itemsRegionName = itemsRegionName.VerifyArgument(nameof(itemsRegionName)).IsNotNullOrEmpty();
            }
            public View(TModel model) : this(model, Regions.Items) { }

            protected void WithItemsRegion(Action<IRegion> action) => WithRegion(_itemsRegionName, action);

            public IEnumerable<TItemView> Populate(IEnumerable<TItemModel> items)
            {
                IEnumerable<TItemView> result = null;
                WithItemsRegion( 
                    itemsRegion =>
                    {
                        itemsRegion.Clear();
                        result = items.Select(item => ActivateItemView(itemsRegion, item)).ToArray();
                    });
                return result;
            }

            protected virtual void AfterItemViewActivated(TItemView view) { }

            private TItemView ActivateItemView(IRegion itemsRegion, TItemModel item)
            {
                var activatedItemView = itemsRegion.ActivateView<TItemView, TItemModel>(item);
                AfterItemViewActivated(activatedItemView);
                return activatedItemView;
            }
        }

        /// <summary>
        /// An abstract class to serve as a base for views which will hold a number of identical repeating child views.
        /// </summary>
        /// <typeparam name="TItemView">The type of the repeater's items view.</typeparam>
        /// <typeparam name="TItemModel">The type of the repeater's items view model.</typeparam>
        public class View<TItemView, TItemModel> : View<object, TItemView, TItemModel> 
            where TItemView : IView<TItemModel>
        {
            protected View(string itemsRegionName) : base(null, itemsRegionName) { }
            public View() : base(null) { }
        }
    }
}
