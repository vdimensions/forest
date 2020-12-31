using System.Collections.Generic;
using System.Collections.Immutable;
using Axle.Verification;

namespace Forest.UI.Forms.Validation
{
    internal sealed class ValidationRuleBuilder : IValidationRuleBuilder
    {
        private readonly IRegion _region;

        public ValidationRuleBuilder(IRegion region) : this(region, new Dictionary<ValidationRule, ValidationState>()) { }
        private ValidationRuleBuilder(IRegion region, IDictionary<ValidationRule, ValidationState> validationStates)
        {
            _region = region;
            ValidationStates = validationStates;
        }

        public IValidationRuleBuilder Compare(string field)
        {
            field.VerifyArgument(nameof(field)).IsNotEmpty();
            var rule = new CompareValidationState(ValidationRule.Compare, new FormFieldReference(_region, field));
            ValidationStates[rule.Rule] = rule;
            return this;
        }
        
        public IValidationRuleBuilder MaxLength(int constraint)
        {
            var rule = new ConstrainedValidationState<int>(ValidationRule.MaxLength, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder MaxValue<T>(T constraint)
        {
            var rule = new ConstrainedValidationState<T>(ValidationRule.MaxValue, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder MinValue<T>(T constraint)
        {
            var rule = new ConstrainedValidationState<T>(ValidationRule.MinValue, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder MinLength(int constraint)
        {
            var rule = new ConstrainedValidationState<int>(ValidationRule.MinLength, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder Required()
        {
            var rule = new ValidationState(ValidationRule.Required);
            ValidationStates[rule.Rule] = rule;
            return this;
        }
        internal IDictionary<ValidationRule, ValidationState> ValidationStates { get; }
    }
}