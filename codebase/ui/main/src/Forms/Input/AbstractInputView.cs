using System;
using System.Diagnostics.CodeAnalysis;
using Forest.Messaging.Propagating;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms.Input
{
    public abstract class AbstractInputView<T, TValue> 
        : LogicalView<T>, 
          IFormInputView<TValue>
    {
        public static class Commands
        {
            public const string UpdateValue = "UpdateValue";
        }
        
        private readonly Func<T, TValue> _valueFn;

        protected AbstractInputView(T model, Func<T, TValue> valueFn) : base(model)
        {
            _valueFn = valueFn;
        }

        /// <inheritdoc />
        public virtual bool Validate(FormField field, TValue value) => field?.Validate(value) ?? true;
        bool IFormInputView.Validate(FormField field, object value) => value is TValue val ? Validate(field, val) : Validate(field, default(TValue));

        [Command(Commands.UpdateValue)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public abstract void UpdateValue(TValue value);

        /// <inheritdoc />
        public virtual TValue Value => _valueFn(Model);
        object IFormInputView.Value => Value;
        Type IFormInputView.ValueType => typeof(TValue);
    }
    
    public abstract class AbstractInputView<TValue> : AbstractInputView<TValue, TValue>
    {
        private static readonly Func<TValue, TValue> ValueIdentity = x => x;
        
        protected AbstractInputView(TValue model) : base(model, ValueIdentity) { }

        public override void UpdateValue(TValue value)
        {
            UpdateModel(_ => value);
            Publish(ValidationStateChanged.Instance, PropagationTargets.Parent);
        }

        public override TValue Value => Model;
    }
}