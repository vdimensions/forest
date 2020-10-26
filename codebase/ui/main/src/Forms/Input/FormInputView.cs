using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms.Input
{
    public class FormInputView<TValue> 
        : LogicalView<TValue>, 
          IFormInputView<TValue>,
          ISupportsAssignFormField
    {
        internal FormInputView(TValue model) : base(model) { }

        /// <inheritdoc />
        public virtual bool Validate(TValue value) => Field?.Validate(value) ?? true;
        bool IFormInputView.Validate(object value) => value is TValue val && Validate(val);

        /// <inheritdoc />
        public event FormInputValueChanged<TValue> ValueChanged;

        /// <inheritdoc />
        public TValue Value => Model;
        object IFormInputView.Value => Value;

        /// <inheritdoc cref="IFormInputView{TValue}.Field" />
        public FormField Field { get; set; }
    }
}