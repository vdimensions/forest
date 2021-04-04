using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms.Input
{
    public abstract class AbstractInputView<T, TValue> 
        : LogicalView<T>, 
          IFormInputView<TValue>, 
          ISupportsAssignFormField
    {
        public static class Commands
        {
            public const string UpdateValue = "UpdateValue";
        }
        
        private FormField _field;
        private TValue _value;

        protected AbstractInputView(T model) : base(model)
        {
        }

        /// <inheritdoc />
        public virtual bool Validate(TValue value) => Field?.Validate(value) ?? true;
        bool IFormInputView.Validate(object value) => value is TValue val && Validate(val);

        [Command(Commands.UpdateValue)]
        public virtual void UpdateValue(TValue value)
        {
            _value = value;
            ValueChanged?.Invoke(value, Validate(value));
        }

        /// <inheritdoc />
        public event FormInputValueChanged<TValue> ValueChanged;

        /// <inheritdoc />
        public FormField Field => _field;
        FormField ISupportsAssignFormField.Field
        {
            set
            {
                _field = value;
                if (_field.DefaultValue is TValue defaultValue)
                {
                    _value = defaultValue;
                }
            }
        }

        /// <inheritdoc />
        public TValue Value => _value;
        object IFormInputView.Value => Value;
    }
    public abstract class AbstractInputView<TValue> : AbstractInputView<TValue, TValue>
    {
        protected AbstractInputView(TValue model) : base(model) { }

        public override void UpdateValue(TValue value)
        {
            base.UpdateValue(value);
            UpdateModel(_ => value);
        }
    }
}