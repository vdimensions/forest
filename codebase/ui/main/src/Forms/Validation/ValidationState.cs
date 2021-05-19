using Forest.Globalization;

namespace Forest.UI.Forms.Validation
{
    [Localized]
    public sealed class ValidationState : IGlobalizationCloneable
    {
        public ValidationState(string name, ValidationRule rule)
        {
            Name = name;
            Rule = rule;
        }

        object IGlobalizationCloneable.Clone()
        {
            return new ValidationState(Name, Rule);
        }

        internal string Name { get; }
        public ValidationRule Rule { get; }
        
        [Localized]
        public string Message { get; set; }
    }
}