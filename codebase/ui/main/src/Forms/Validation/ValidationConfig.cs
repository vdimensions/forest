using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public class ValidationConfig : ISupportsValidationStateChange, IGlobalizationCloneable
    {
        internal ValidationConfig(ValidationRule rule)
        {
            Rule = rule;
        }
        
        object IGlobalizationCloneable.Clone()
        {
            return new ValidationConfig(Rule)
            {
                IsValid = IsValid,
                Message = Message
            };
        }

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