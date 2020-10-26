using System.Runtime.Serialization;
using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public class ValidationState : ISupportsValidationStateChange
    {
        internal ValidationState(ValidationRule rule)
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
        
        [Localized]
        [IgnoreDataMember]
        public string Message { get; set; }
    }
}