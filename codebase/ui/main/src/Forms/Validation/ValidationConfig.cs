namespace Forest.UI.Forms.Validation
{
    public class ValidationConfig : ISupportsValidationStateChange
    {
        internal ValidationConfig(ValidationRule rule)
        {
            Rule = rule;
        }

        public ValidationRule Rule { get; }
        
        public bool? IsValid { get; private set; }
        bool? ISupportsValidationStateChange.IsValid
        {
            get => IsValid;
            set => IsValid = value;
        }
    }
}