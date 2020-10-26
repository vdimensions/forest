using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axle.Verification;

namespace Forest.UI.Common
{
    /// <summary>
    /// An abstract class to serve as a base for views which will hold a number of identical repeating child views.
    /// </summary>
    /// <typeparam name="TItemView">The type of the repeater's items view.</typeparam>
    /// <typeparam name="TItemModel">The type of the repeater's items view model.</typeparam>
    public class Repeater<TModel, TItemView, TItemModel> : LogicalView<TModel> where TItemView: IView<TItemModel>
    {
        private static class Regions
        {
            public const string Items = "Items";
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _itemsRegionName;

        protected Repeater(TModel model, string itemsRegionName) : base(model)
        {
            _itemsRegionName = itemsRegionName.VerifyArgument(nameof(itemsRegionName)).IsNotNullOrEmpty();
        }
        public Repeater(TModel model) : this(model, Regions.Items) { }

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
    /// A container view which will hold a number of identical repeating child views.
    /// </summary>
    /// <typeparam name="TItemView">The type of the repeater's items view.</typeparam>
    /// <typeparam name="TItemModel">The type of the repeater's items view model.</typeparam>
    [View(Name)]
    public sealed class Repeater<TItemView, TItemModel> : Repeater<object, TItemView, TItemModel> where TItemView : IView<TItemModel>
    {
        public const string Name = "Repeater";
        
        public Repeater() : base(null) { }
    }
}
