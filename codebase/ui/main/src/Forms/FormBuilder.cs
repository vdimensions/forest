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
        private readonly string _formName;
        private readonly ImmutableDictionary<string, Tuple<FormField, Type, Type>> _fields;

        internal FormBuilder(IRegion region, string formName) : this(region, formName, ImmutableDictionary<string, Tuple<FormField, Type, Type>>.Empty) { }
        private FormBuilder(IRegion region, string formName, ImmutableDictionary<string, Tuple<FormField, Type, Type>> fields)
        {
            _region = region;
            _formName = formName;
            _fields = fields;
        }

        public IFormBuilder AddField(
            string name, 
            Type inputViewType, 
            Type inputValueType,
            Action<IValidationRuleBuilder> buildValidationRules = null)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            inputViewType.VerifyArgument(nameof(inputViewType)).IsNotNull().Is<IFormInputView>();
            inputValueType.VerifyArgument(nameof(inputValueType)).IsNotNull();
            
            var validationRulesBuilder = new ValidationRuleBuilder(_region);
            buildValidationRules?.Invoke(validationRulesBuilder);
            return new FormBuilder(
                _region, 
                _formName,
                _fields.Remove(name).Add(
                    name, 
                    Tuple.Create(
                        new FormField($"{_formName}.{name}", validationRulesBuilder.ValidationStates.ToImmutableDictionary()), 
                        inputViewType,
                        inputValueType)));
        }

        public IFormBuilder AddField<TFormInputView, TValue>(string name, Action<IValidationRuleBuilder> buildValidationRules = null) 
            where TFormInputView : IFormInputView<TValue>
        {
            return AddField(name, typeof(TFormInputView), typeof(TValue), buildValidationRules);
        }

        private IEnumerable<KeyValuePair<string, IFormInputView>> DoBuild()
        {
            _region.Clear();
            var introspector = new TypeIntrospector(typeof(FormFieldView<,>));
            foreach (var kvp in _fields)
            {
                var name = kvp.Key;
                var formFieldData = kvp.Value;
                var field = formFieldData.Item1;
                var inputViewType = formFieldData.Item2;
                var inputValueType = formFieldData.Item3;
                var viewType = introspector.GetGenericTypeDefinition().MakeGenericType(inputViewType, inputValueType)
                    .Introspect()
                    .IntrospectedType;
                yield return new KeyValuePair<string, IFormInputView>(name, ((IFormFieldView) _region.ActivateView(viewType, field)).FormInputView);
            }
        }

        public IDictionary<string, IFormInputView> Build() => DoBuild().ToImmutableDictionary();
    }
}