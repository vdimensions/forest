using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public sealed class CompareValidationState : ValidationState
    {
        internal CompareValidationState(ValidationRule rule, FormFieldReference target) : base(rule)
        {
            Target = target;
        }

        internal FormFieldReference Target { get; }

        public string Name => Target.View?.Model?.Name;
    }
}