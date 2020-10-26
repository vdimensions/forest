using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Axle.Reflection;
using Axle.Verification;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    internal sealed class FormBuilder : IFormBuilder
    {
        private readonly IRegion _region;
        private readonly ImmutableDictionary<string, Tuple<FormField, Type, Type>> _fields;

        internal FormBuilder(IRegion region) : this(region, ImmutableDictionary<string, Tuple<FormField, Type, Type>>.Empty) { }
        private FormBuilder(IRegion region, ImmutableDictionary<string, Tuple<FormField, Type, Type>> fields)
        {
            _region = region;
            _fields = fields;
        }

        public IFormBuilder AddField(
            string name, 
            Type inputViewType, 
            Type inputValueType,
            Action<IValidationRuleBuilder> buildValidationRules = null)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            inputViewType.VerifyArgument(nameof(inputViewType)).IsNotNull().IsOfType<IFormInputView>();
            inputValueType.VerifyArgument(nameof(inputValueType)).IsNotNull();
            
            var validationRulesBuilder = new ValidationRuleBuilder(_region);
            buildValidationRules?.Invoke(validationRulesBuilder);
            return new FormBuilder(
                _region, 
                _fields.Remove(name).Add(
                    name, 
                    Tuple.Create(
                        new FormField(name, validationRulesBuilder.ValidationStates), 
                        inputViewType,
                        inputValueType)));
        }

        public IFormBuilder AddField<TFormInputView, TValue>(string name, Action<IValidationRuleBuilder> buildValidationRules = null) 
            where TFormInputView : IFormInputView<TValue>
        {
            return AddField(name, typeof(TFormInputView), typeof(TValue), buildValidationRules);
        }

        public IEnumerable<IFormInputView> Build()
        {
            _region.Clear();
            var introspector = new TypeIntrospector(typeof(FormFieldView<,>));
            foreach (var formFieldData in _fields.Values)
            {
                var field = formFieldData.Item1;
                var inputViewType = formFieldData.Item2;
                var inputValueType = formFieldData.Item3;
                var viewType = introspector.GetGenericTypeDefinition().MakeGenericType(inputViewType, inputValueType)
                    .Introspect()
                    .IntrospectedType;
                yield return (IFormInputView) _region.ActivateView(viewType, field);
            }
        }
    }
}