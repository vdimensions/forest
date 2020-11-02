using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms.Input.Select
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public class MultiSelectView<TItemView, TItemModel> 
        : AbstractSelectView<object, TItemView, TItemModel>, 
          IFormInputView<TItemModel[]>
        where TItemView : IView<TItemModel>
    {
        private TItemModel[] _value = new TItemModel[0];
        private const string Name = "MultiSelect";
        
        [ViewRegistryCallback]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void RegisterViews(IForestViewRegistry viewRegistry)
        {
            viewRegistry.Register<SelectOptionView<TItemView, TItemModel>>();
        }
        
        public MultiSelectView(IEqualityComparer<TItemModel> itemComparer) : base(null, itemComparer) { }
        public MultiSelectView() : base(null) { }
        
        public IEnumerable<TItemView> Populate(TItemModel[] items, TItemModel[] selectedItems)
        {
            if (items.Length == 0)
            {
                return Enumerable.Empty<TItemView>();
            }
            return PopulateAndSelect(items, selectedItems);
        }

        /// <inheritdoc />
        protected sealed override void HandleSelectionChanged(
            IEnumerable<SelectOptionView<TItemView, TItemModel>> allOptionViews, 
            SelectOptionView<TItemView, TItemModel> toggledSelectOptionView, 
            IEqualityComparer<TItemModel> itemComparer)
        {
            var selectedItems = new List<TItemModel>();
            foreach (var optionView in allOptionViews)
            {
                var isTheCurrentItem = itemComparer.Equals(optionView.Model.Value, toggledSelectOptionView.Model.Value);
                if (isTheCurrentItem)
                {
                    optionView.UpdateModel(m => new SelectOption<TItemModel>(m.Value, !m.Selected)); 
                }

                if (optionView.Model.Selected)
                {
                    selectedItems.Add(optionView.Model.Value);
                }
            }
            ValueChanged?.Invoke(_value = selectedItems.ToArray(), Validate(Value));
        }

        /// <inheritdoc />
        public bool Validate(TItemModel[] value)
        {
            if (Field == null)
            {
                return true;
            }
            var isValid = true;
            foreach (var kvp in Field.Validation)
            {
                ISupportsValidationStateChange setValidationState = kvp.Value; 
                switch (kvp.Key)
                {
                    case ValidationRule.Required:
                        setValidationState.IsValid = value.Length == 0;
                        break;
                }
                isValid = kvp.Value.IsValid.GetValueOrDefault(true);
            }
            return isValid;
        }
        bool IFormInputView.Validate(object value) => value is TItemModel[] val && Validate(val);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            ValueChanged = null;
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public event FormInputValueChanged<TItemModel[]> ValueChanged;

        /// <inheritdoc />
        public TItemModel[] Value => _value;
        object IFormInputView.Value => Value;
    }
}