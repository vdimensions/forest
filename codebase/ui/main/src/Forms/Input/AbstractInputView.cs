using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms.Input
{
    public abstract class AbstractInputView<T, TValue> 
        : LogicalView<T>, 
          IFormInputView<TValue>, 
          ISupportsAssignFormField
    {
        private FormField _field;

        /// <inheritdoc />
        public virtual bool Validate(TValue value) => Field?.Validate(value) ?? true;
        bool IFormInputView.Validate(object value) => value is TValue val && Validate(val);

        /// <inheritdoc />
        public event FormInputValueChanged<TValue> ValueChanged;

        /// <inheritdoc />
        public FormField Field => _field;
        FormField ISupportsAssignFormField.Field { set => _field = value; }

        /// <inheritdoc />
        public abstract TValue Value { get; }
        object IFormInputView.Value => Value;

        protected AbstractInputView(T model) : base(model)
        {
        }
    }
    public abstract class AbstractInputView<TValue> : AbstractInputView<TValue, TValue>
    {
        protected AbstractInputView(TValue model) : base(model) { }

        /// <inheritdoc />
        public override TValue Value => Model;
    }
}