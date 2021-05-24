using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public sealed class ConstrainedValidationState<T> : ValidationState
    {
        internal ConstrainedValidationState(ValidationRule rule, T constraint) : base(rule)
        {
            Constraint = constraint;
        }

        public T Constraint { get; }
    }
}