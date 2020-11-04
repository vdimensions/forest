using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.UI.Common;

namespace Forest.UI.Forms.Input.Select
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class AbstractSelectView<TSelectModel, TItemView, TItemModel> 
        : Repeater<TSelectModel, TItemView, SelectOption<TItemModel>>,
          ISupportsAssignFormField
        where TItemView : AbstractSelectOptionView<TItemModel>
    {
        private FormField _field;
        private IEnumerable<TItemView> _optionViews;
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
                result.AddLast(view);
            }
            return result;
        }

        protected abstract void HandleSelectionChanged(
            IEnumerable<TItemView> allOptionViews,
            TItemView toggledSelectOptionView,
            IEqualityComparer<TItemModel> itemComparer);

        private void Toggled(AbstractSelectOptionView<TItemModel> selectedSelectOptionView) => Toggled((TItemView) selectedSelectOptionView);
        private void Toggled(TItemView selectedSelectOptionView)
        {
            HandleSelectionChanged(_optionViews, selectedSelectOptionView, _itemComparer);
        }

        protected IEqualityComparer<TItemModel> ItemComparer => _itemComparer;
        
        /// <inheritdoc cref="IFormInputView{TValue}.Field" />
        public FormField Field => _field;
        FormField ISupportsAssignFormField.Field { set => _field = value; }
    }
}