using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.UI.Common;

namespace Forest.UI.Forms.Input.Select
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class AbstractSelectView<TSelectModel, TItemView, TItemModel> 
        : Repeater<TSelectModel, SelectOptionView<TItemView, TItemModel>, SelectOption<TItemModel>>
        where TItemView : IView<TItemModel>
    {
        private IEnumerable<SelectOptionView<TItemView, TItemModel>> _optionViews;
        private readonly IEqualityComparer<TItemModel> _itemComparer;

        protected AbstractSelectView(TSelectModel model, IEqualityComparer<TItemModel> itemComparer) : base(model)
        {
            _itemComparer = itemComparer;
        }
        protected AbstractSelectView(TSelectModel model) : this(model, EqualityComparer<TItemModel>.Default) { }

        protected IEnumerable<TItemView> PopulateAndSelect(TItemModel[] items, params TItemModel[] selectedItems)
        {
            var selectedItemsSet = new HashSet<TItemModel>(selectedItems, _itemComparer);
            var result = new LinkedList<TItemView>();
            _optionViews = Populate(items.Select((x) => new SelectOption<TItemModel>(x, selectedItemsSet.Contains(x))));
            foreach (var view in _optionViews)
            {
                view.Toggled += Toggled;
                result.AddLast(view.ContentView);
            }
            return result;
        }

        protected abstract void HandleSelectionChanged(
            IEnumerable<SelectOptionView<TItemView, TItemModel>> allOptionViews,
            SelectOptionView<TItemView, TItemModel> toggledSelectOptionView,
            IEqualityComparer<TItemModel> itemComparer);

        private void Toggled(SelectOptionView<TItemView, TItemModel> selectedSelectOptionView)
        {
            HandleSelectionChanged(_optionViews, selectedSelectOptionView, _itemComparer);
        }

        protected IEqualityComparer<TItemModel> ItemComparer => _itemComparer;
    }
}