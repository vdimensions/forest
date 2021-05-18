using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Axle.Collections.Generic;
using Axle.Text.Expressions.Regular;
using Axle.Verification;

namespace Forest.UI.Forms.Validation
{
    public static class FormFieldExtensions
    {
        public static bool Validate<TValue>(
            this FormField formField, 
            TValue value,
            TValue[] emptyValues = null,
            IEqualityComparer<TValue> equalityComparer = null,
            IComparer<TValue> comparer = null)
        {
            formField.VerifyArgument(nameof(formField)).IsNotNull();
            emptyValues = emptyValues ?? new[]{ default(TValue) };
            equalityComparer = equalityComparer ?? EqualityComparer<TValue>.Default;
            comparer = comparer ?? Comparer<TValue>.Default;
            
            var isValid = true;
            var isEmpty = emptyValues.Any(emptyValue => equalityComparer.Equals(value, emptyValue));

            foreach (var kvp in formField.Validation)
            {
                var validationRule = kvp.Key;
                var validationState = kvp.Value;
                ISupportsValidationStateChange validationStateChange = validationState;
                switch (validationRule)
                {
                    case ValidationRule.Required:
                        validationStateChange.IsValid = !isEmpty;
                        break;
                    case ValidationRule.MaxValue:
                    case ValidationRule.MinValue:
                        if (isEmpty)
                        {
                            break;
                        }
                        if (validationStateChange is ConstrainedValidationState<TValue> valueRangeValidationState)
                        {
                            validationStateChange.IsValid = validationRule == ValidationRule.MaxValue
                                ? comparer.Compare(value, valueRangeValidationState.Constraint) <= 0
                                : comparer.Compare(value, valueRangeValidationState.Constraint) >= 0;
                        }
                        break;
                    case ValidationRule.MinLength:
                    case ValidationRule.MaxLength:
                        if (isEmpty)
                        {
                            break;
                        }
                        if (value is IEnumerable enumerable && validationStateChange is ConstrainedValidationState<int> lengthValidationState)
                        {
                            var length = new GenericEnumerable<object>(enumerable).Count();
                            validationStateChange.IsValid = validationRule == ValidationRule.MaxLength
                                ? length >= lengthValidationState.Constraint
                                : length <= lengthValidationState.Constraint;
                        }
                        break;
                    case ValidationRule.Regex:
                        if (isEmpty)
                        {
                            break;
                        }
                        if (value is string str)
                        {
                            switch (validationState)
                            {
                                case ConstrainedValidationState<string> regexStringConstrainedValidationState:
                                    validationStateChange.IsValid = Regex.IsMatch(str, regexStringConstrainedValidationState.Constraint);
                                    break;
                                case ConstrainedValidationState<Regex> regexConstrainedValidationState:
                                    validationStateChange.IsValid = regexConstrainedValidationState.Constraint.IsMatch(str);
                                    break;
                                case ConstrainedValidationState<IRegularExpression> axleRegexConstrainedValidationState:
                                    validationStateChange.IsValid = axleRegexConstrainedValidationState.Constraint.IsMatch(str);
                                    break;
                            }
                        }
                        break;
                    case ValidationRule.Compare:
                        if (isEmpty)
                        {
                            break;
                        }
                        if (validationState is CompareValidationState compareValidationState)
                        {
                            var view = compareValidationState.Target.View;
                            if (view == null)
                            {
                                break;
                            }
                            validationStateChange.IsValid = view.FormInputView.Value is TValue v && equalityComparer.Equals(value, v);
                        }
                        break;
                    default:
                        break;
                }
                isValid &= validationStateChange.IsValid.GetValueOrDefault(true);
            }

            return isValid;
        }
    }
}