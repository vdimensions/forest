using System.Collections.Generic;
using System.Linq;
using Forest.Globalization;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    /// <summary>
    /// The model for a <see cref="FormFieldView{TInput,TValue}"/>.
    /// </summary>
    [Localized]
    public sealed class FormField : IGlobalizationCloneable
    {
        internal FormField(string name, object defaultValue, IReadOnlyDictionary<ValidationRule, ValidationState> validation)
        {
            Name = name;
            DefaultValue = defaultValue;
            Validation = validation;
        }

        object IGlobalizationCloneable.Clone()
        {
            return new FormField(
                Name, 
                DefaultValue,
                new Dictionary<ValidationRule, ValidationState>(
                    Validation.ToDictionary(
                        x => x.Key, 
                        x => x.Value)))
            {
                Label = Label
            };
        }

        /// <summary>
        /// Gets the localized label of the current form field.
        /// </summary>
        [Localized]
        public string Label { get; set; }
        
        /// <summary>
        /// Gets the name of the current form field.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the default value for the form field, in case specified.
        /// </summary>
        public object DefaultValue { get; }
        
        /// <summary>
        /// Gets the validation rules associated with the current form field.
        /// </summary>
        [Localized]
        public IReadOnlyDictionary<ValidationRule, ValidationState> Validation { get; }
    }
}