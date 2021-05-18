using System;
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
        private readonly Func<T, TValue> _valueFn;

        protected AbstractInputView(T model, Func<T, TValue> valueFn) : base(model)
        {
            _valueFn = valueFn;
        }

        /// <inheritdoc />
        public virtual bool Validate(TValue value) => Field?.Validate(value) ?? true;
        bool IFormInputView.Validate(object value) => value is TValue val ? Validate(val) : Validate(default(TValue));

        [Command(Commands.UpdateValue)]
        public abstract void UpdateValue(TValue value);

        /// <inheritdoc />
        public FormField Field => _field;

        FormField ISupportsAssignFormField.Field
        {
            set => _field = value;
        }

        /// <inheritdoc />
        public virtual TValue Value => _valueFn(Model);
        object IFormInputView.Value => Value;
        Type IFormInputView.ValueType => typeof(TValue);
    }
    public abstract class AbstractInputView<TValue> : AbstractInputView<TValue, TValue>
    {
        private static readonly  Func<TValue, TValue> ValueIdentity = x => x;
        
        protected AbstractInputView(TValue model) : base(model, ValueIdentity) { }

        public override void UpdateValue(TValue value)
        {
            UpdateModel(_ => value);
        }

        public override TValue Value => Model;
    }
}