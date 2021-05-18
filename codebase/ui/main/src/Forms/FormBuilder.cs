using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Collections.Immutable;
using Axle.Extensions.String;
using Axle.Reflection;
using Axle.Verification;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    using FormFieldDataTuple = Tuple<ImmutableDictionary<string, IFormInputView>, ImmutableList<string>>;
    
    internal sealed class FormBuilder : IFormBuilder
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.Ordinal;
        private static Func<KeyValuePair<string, IFormInputView>, string> ToFieldName(string formName)
        {
            return (x) => x.Key.TakeAfterFirst(formName).TrimStart('.');
        }

        private readonly IRegion _region;
        private readonly string _formName;
        private readonly ImmutableDictionary<string, IFormInputView> _inputs;
        private readonly ImmutableList<string> _fieldNames;

        internal FormBuilder(IRegion region, string formName) 
            : this(region, formName, null) { }
        private FormBuilder(
            IRegion region, 
            string formName, 
            FormFieldDataTuple formFieldData)
        {
            _region = region;
            _formName = formName;
            if (formFieldData == null)
            {
                var pairs = region.Views
                    .OfType<IFormFieldView>()
                    .Select(v => new KeyValuePair<string, IFormInputView>(
                        v.FormInputView.Field.Name,
                        v.FormInputView))
                    .ToArray();
                var fields = ImmutableDictionary.CreateRange(Comparer, pairs);
                var toFieldName = ToFieldName(formName);
                var fieldNames = ImmutableList.CreateRange(pairs.Select(toFieldName));
                _inputs = fields;
                _fieldNames = fieldNames;
            }
            else
            {
                _inputs = formFieldData.Item1;
                _fieldNames = formFieldData.Item2;
            }
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
            
            var field = new FormField($"{_formName}.{name}", defaultValue, validationRulesBuilder.ValidationStates.ToImmutableDictionary());
            
            var introspector = new TypeIntrospector(typeof(FormFieldView<,>));
            var viewType = introspector
                .GetGenericTypeDefinition()
                .MakeGenericType(inputViewType, inputValueType)
                .Introspect()
                .IntrospectedType;
            var view = ((IFormFieldView) _region.ActivateView(viewType, field)).FormInputView;
            return new FormBuilder(
                _region, 
                _formName,
                Tuple.Create(
                    _inputs.Remove(name).Add(name, view),
                    _fieldNames.Remove(name, Comparer).Add(name))
                );
        }

        public IFormBuilder AddField<TFormInputView, TValue>(
                string name, 
                TValue defaultValue = default(TValue), 
                Action<IValidationRuleBuilder> buildValidationRules = null) 
            where TFormInputView : IFormInputView<TValue>
        {
            return AddField(name, typeof(TFormInputView), typeof(TValue), defaultValue, buildValidationRules);
        }
        

        public IReadOnlyDictionary<string, object> GetValues()
        {
            return _inputs.ToDictionary(x => x.Key, x => x.Value.Value, Comparer);
        }
        
        public bool Submit(
            out IReadOnlyDictionary<string, object> values,
            out IReadOnlyDictionary<string, ValidationRule[]> errors)
        {
            var collectedValues = ImmutableDictionary.Create<string, object>(Comparer);
            var collectedErrors = ImmutableDictionary.Create<string, ValidationRule[]>(Comparer);
            var toFieldName = ToFieldName(_formName);
            foreach (var kvp in Inputs)
            {
                var fieldName = toFieldName(kvp);
                var input = kvp.Value;
                if (input.Validate(input.Value))
                {
                    collectedValues = collectedValues.Add(fieldName, input.Value);
                }
                else
                {
                    var violatedRules = input.Field.Validation
                        .Where(x => !x.Value.IsValid.GetValueOrDefault(true))
                        .Select(x => x.Key)
                        .ToArray();
                    collectedErrors = collectedErrors.Add(
                        fieldName, 
                        violatedRules);
                }
            }

            errors = collectedErrors;
            values = collectedErrors.Count > 0 
                ? collectedValues.Clear() 
                : collectedValues;
            return errors.Count == 0;
        }

        public IReadOnlyDictionary<string, IFormInputView> Inputs => _inputs;
    }
}