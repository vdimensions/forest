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
        protected void WithItemsRegion<T>(Action<IRegion, T> action, T arg) => WithRegion(_itemsRegionName, action, arg);
        protected TResult WithItemsRegion<TResult>(Func<IRegion, TResult> func) => WithRegion(_itemsRegionName, func);
        protected TResult WithItemsRegion<T, TResult>(Func<IRegion, T, TResult> func, T arg) => WithRegion(_itemsRegionName, func, arg);

        public IEnumerable<TItemView> Populate(IEnumerable<TItemModel> items)
        {
            IEnumerable<TItemView> result = WithItemsRegion( 
                (itemsRegion, x) =>
                {
                    itemsRegion.Clear();
                    return items.Select(item => x.ActivateItemView(itemsRegion, item)).ToArray();
                },
                this);
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
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class Repeater<TItemView, TItemModel> : Repeater<object, TItemView, TItemModel> where TItemView : IView<TItemModel>
    {
        public const string Name = "Repeater";
        
        public Repeater() : base(null) { }
    }
}
