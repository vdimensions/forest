using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms.Input.Select
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public class SelectView<TItemView, TItemModel> 
        : AbstractSelectView<object, TItemView, TItemModel>,
          IFormInputView<TItemModel>
        where TItemView : IView<TItemModel>
    {
        private const string Name = "Select";
        
        [ViewRegistryCallback]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void RegisterViews(IForestViewRegistry viewRegistry)
        {
            viewRegistry.Register<SelectOptionView<TItemView, TItemModel>>();
        }
        
        public SelectView(IEqualityComparer<TItemModel> itemComparer) : base(null, itemComparer) { }
        public SelectView() : base(null) { }
        
        public IEnumerable<TItemView> Populate(TItemModel[] items)
        {
            if (items.Length == 0)
            {
                return Enumerable.Empty<TItemView>();
            }
            return PopulateAndSelect(items, items[0]);
        }
        public IEnumerable<TItemView> Populate(TItemModel[] items, TItemModel selectedItem)
        {
            if (items.Length == 0)
            {
                return Enumerable.Empty<TItemView>();
            }
            return PopulateAndSelect(items, selectedItem);
        }

        /// <inheritdoc />
        protected sealed override void HandleSelectionChanged(
            IEnumerable<SelectOptionView<TItemView, TItemModel>> allOptionViews, 
            SelectOptionView<TItemView, TItemModel> toggledSelectOptionView, 
            IEqualityComparer<TItemModel> itemComparer)
        {
            var value = toggledSelectOptionView.Model.Value;
            Value = value;
            if (toggledSelectOptionView.Model.Selected)
            {
                //
                // Single select view does not support de-select
                //
                return;
            }
            foreach (var optionView in allOptionViews)
            {
                var isTheCurrentItem = itemComparer.Equals(optionView.Model.Value, toggledSelectOptionView.Model.Value);
                optionView.UpdateModel(m => new SelectOption<TItemModel>(m.Value, isTheCurrentItem));
            }
            ValueChanged?.Invoke(toggledSelectOptionView.ContentView.Model, Validate(value));
        }

        /// <inheritdoc />
        public bool Validate(TItemModel value) => Field?.Validate(value, default(TItemModel), ItemComparer) ?? true;
        bool IFormInputView.Validate(object value) => value is TItemModel item && Validate(item);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            ValueChanged = null;
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public event FormInputValueChanged<TItemModel> ValueChanged;

        /// <inheritdoc />
        public TItemModel Value { get; private set; }
        object IFormInputView.Value => Value;
    }
}