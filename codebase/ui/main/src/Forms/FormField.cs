using System;
using System.Collections.Generic;
using Forest.Globalization;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    /// <summary>
    /// The model for a <see cref="FormFieldView{TInput,TValue}"/>.
    /// </summary>
    [Localized]
    public sealed class FormField : ICloneable
    {
        internal FormField(string name, IDictionary<ValidationRule, ValidationState> validation)
        {
            Name = name;
            Validation = validation;
        }

        object ICloneable.Clone()
        {
            return new FormField(Name, Validation);
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
        /// Gets the validation rules associated with the current form field.
        /// </summary>
        [Localized]
        public IDictionary<ValidationRule, ValidationState> Validation { get; }
    }
}