using System;
using System.Collections.Generic;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    /// <summary>
    /// An interface for a form builder. The form builder is attached to a <see cref="IRegion"/>
    /// instance and is used to insert form field views within.
    /// </summary>
    public interface IFormBuilder
    {
        IFormBuilder AddField(
            string name, 
            Type inputViewType, 
            Type inputValueType, 
            object defaultValue = null,
            Action<IValidationRuleBuilder> buildValidationRules = null);
        IFormBuilder AddField<TFormInputView, TValue>(
            string name,
            TValue defaultValue = default(TValue),
            Action<IValidationRuleBuilder> buildValidationRules = null) where TFormInputView : IFormInputView<TValue>;
    }
}