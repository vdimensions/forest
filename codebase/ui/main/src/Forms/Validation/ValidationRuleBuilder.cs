using System.Collections.Generic;
using Axle.Collections.Immutable;
using Axle.Verification;

namespace Forest.UI.Forms.Validation
{
    internal sealed class ValidationRuleBuilder : IValidationRuleBuilder
    {
        private readonly IRegion _region;

        public ValidationRuleBuilder(IRegion region) : this(region, new Dictionary<ValidationRule, ValidationConfig>()) { }
        private ValidationRuleBuilder(IRegion region, IDictionary<ValidationRule, ValidationConfig> validationStates)
        {
            _region = region;
            ValidationStates = validationStates;
        }

        public IValidationRuleBuilder Compare(string field)
        {
            field.VerifyArgument(nameof(field)).IsNotEmpty();
            var rule = new CompareValidationConfig(ValidationRule.Compare, new FormFieldReference(_region, field));
            ValidationStates[rule.Rule] = rule;
            return this;
        }
        
        public IValidationRuleBuilder MaxLength(int constraint)
        {
            var rule = new ConstrainedValidationConfig<int>(ValidationRule.MaxLength, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder MaxValue<T>(T constraint)
        {
            var rule = new ConstrainedValidationConfig<T>(ValidationRule.MaxValue, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder MinValue<T>(T constraint)
        {
            var rule = new ConstrainedValidationConfig<T>(ValidationRule.MinValue, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder MinLength(int constraint)
        {
            var rule = new ConstrainedValidationConfig<int>(ValidationRule.MinLength, constraint);
            ValidationStates[rule.Rule] = rule;
            return this;
        }

        public IValidationRuleBuilder Required()
        {
            var rule = new ValidationConfig(ValidationRule.Required);
            ValidationStates[rule.Rule] = rule;
            return this;
        }
        internal IDictionary<ValidationRule, ValidationConfig> ValidationStates { get; }
    }
}