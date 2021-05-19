using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public sealed class ConstrainedValidationConfig<T> : ValidationConfig
    {
        internal ConstrainedValidationConfig(ValidationRule rule, T constraint) : base(rule)
        {
            Constraint = constraint;
        }

        public T Constraint { get; }
    }
}