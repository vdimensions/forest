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
        public abstract class View<TItemView, TItemModel> : LogicalView where TItemView: IView<TItemModel>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly string _itemsRegionName;

            protected View(string itemsRegionName) : base()
            {
                _itemsRegionName = itemsRegionName.VerifyArgument(nameof(itemsRegionName)).IsNotNullOrEmpty();
            }
            protected View() : this(Regions.Items) { }

            protected virtual IEnumerable<TItemView> Populate(IEnumerable<TItemModel> items)
            {
                var itemsRegion = FindRegion(_itemsRegionName).Clear();
                return items.Select(item => itemsRegion.ActivateView<TItemView, TItemModel>(item)).ToList();
            }
        }
    }
}
