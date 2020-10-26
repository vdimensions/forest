using System.Collections.Immutable;
using Axle.Verification;

namespace Forest.UI.Forms.Validation
{
    internal sealed class ValidationRuleBuilder : IValidationRuleBuilder
    {
        private readonly IRegion _region;

        public ValidationRuleBuilder(IRegion region) : this(region, ImmutableDictionary<ValidationRule, ValidationState>.Empty) { }
        private ValidationRuleBuilder(IRegion region, ImmutableDictionary<ValidationRule, ValidationState> validationStates)
        {
            _region = region;
            ValidationStates = validationStates;
        }

        public IValidationRuleBuilder Compare(string field)
        {
            field.VerifyArgument(nameof(field)).IsNotEmpty();
            var rule = new CompareValidationState(ValidationRule.Compare, new FormFieldReference(_region, field));
            return new ValidationRuleBuilder(
                _region,
                ValidationStates.Remove(rule.Rule).Add(rule.Rule, rule));
        }
        
        public IValidationRuleBuilder MaxLength(int constraint)
        {
            var rule = new ConstrainedValidationState<int>(ValidationRule.MaxLength, constraint);
            return new ValidationRuleBuilder(
                _region,
                ValidationStates.Remove(rule.Rule).Add(rule.Rule, rule));
        }

        public IValidationRuleBuilder MaxValue<T>(T constraint)
        {
            var rule = new ConstrainedValidationState<T>(ValidationRule.MaxValue, constraint);
            return new ValidationRuleBuilder(
                _region,
                ValidationStates.Remove(rule.Rule).Add(rule.Rule, rule));
        }

        public IValidationRuleBuilder MinValue<T>(T constraint)
        {
            var rule = new ConstrainedValidationState<T>(ValidationRule.MinValue, constraint);
            return new ValidationRuleBuilder(
                _region,
                ValidationStates.Remove(rule.Rule).Add(rule.Rule, rule));
        }

        public IValidationRuleBuilder MinLength(int constraint)
        {
            var rule = new ConstrainedValidationState<int>(ValidationRule.MinLength, constraint);
            return new ValidationRuleBuilder(
                _region,
                ValidationStates.Remove(rule.Rule).Add(rule.Rule, rule));
        }

        public IValidationRuleBuilder Required()
        {
            var rule = new ValidationState(ValidationRule.Required);
            return new ValidationRuleBuilder(
                _region,
                ValidationStates.Remove(rule.Rule).Add(rule.Rule, rule));
        }
        internal ImmutableDictionary<ValidationRule, ValidationState> ValidationStates { get; }
    }
}