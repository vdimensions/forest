using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public class ValidationState : ISupportsValidationStateChange, IGlobalizationCloneable
    {
        internal ValidationState(ValidationRule rule)
        {
            Rule = rule;
        }

        object IGlobalizationCloneable.Clone() => new ValidationState(Rule) { IsValid = IsValid };

        public ValidationRule Rule { get; }
        
        public bool? IsValid { get; private set; }
        bool? ISupportsValidationStateChange.IsValid
        {
            get => IsValid;
            set => IsValid = value;
        }
        
        [Localized]
        public string Message { get; set; }
    }
}