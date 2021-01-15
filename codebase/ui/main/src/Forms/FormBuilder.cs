using System;
using System.Collections.Generic;
using Axle.Reflection;
using Axle.Verification;
using Forest.Collections.Immutable;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    internal sealed class FormBuilder : IFormBuilder
    {
        private readonly IRegion _region;
        private readonly string _formName;
        private readonly ImmutableDictionary<string, Tuple<FormField, Type, Type>> _fields;
        private readonly ImmutableList<string> _fieldNames;

        internal FormBuilder(IRegion region, string formName) 
            : this(region, formName, ImmutableDictionary<string, Tuple<FormField, Type, Type>>.Empty, ImmutableList<string>.Empty) { }
        private FormBuilder(IRegion region, string formName, ImmutableDictionary<string, Tuple<FormField, Type, Type>> fields, ImmutableList<string> fieldNames)
        {
            _region = region;
            _formName = formName;
            _fields = fields;
            _fieldNames = fieldNames;
        }

        public IFormBuilder AddField(
            string name, 
            Type inputViewType, 
            Type inputValueType,
            object defaultValue,
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
                        new FormField($"{_formName}.{name}", defaultValue, validationRulesBuilder.ValidationStates.ToImmutableDictionary()), 
                        inputViewType,
                        inputValueType)),
                _fieldNames.Remove(name).Add(name));
        }

        public IFormBuilder AddField<TFormInputView, TValue>(string name, TValue defaultValue = default(TValue), Action<IValidationRuleBuilder> buildValidationRules = null) 
            where TFormInputView : IFormInputView<TValue>
        {
            return AddField(name, typeof(TFormInputView), typeof(TValue), defaultValue, buildValidationRules);
        }

        private IEnumerable<KeyValuePair<string, IFormInputView>> DoBuild()
        {
            _region.Clear();
            var introspector = new TypeIntrospector(typeof(FormFieldView<,>));
            foreach (var name in _fieldNames)
            {
                var formFieldData = _fields[name];
                var field = formFieldData.Item1;
                var inputViewType = formFieldData.Item2;
                var inputValueType = formFieldData.Item3;
                var viewType = introspector.GetGenericTypeDefinition().MakeGenericType(inputViewType, inputValueType)
                    .Introspect()
                    .IntrospectedType;
                yield return new KeyValuePair<string, IFormInputView>(name, ((IFormFieldView) _region.ActivateView(viewType, field)).FormInputView);
            }
        }

        public IReadOnlyDictionary<string, IFormInputView> Build() => DoBuild().ToImmutableDictionary();
    }
}