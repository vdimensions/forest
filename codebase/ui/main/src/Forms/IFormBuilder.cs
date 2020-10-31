using System;
using System.Collections.Generic;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    public interface IFormBuilder
    {
        IFormBuilder AddField(
            string name, 
            Type inputViewType, 
            Type inputValueType, 
            Action<IValidationRuleBuilder> buildValidationRules = null);
        IFormBuilder AddField<TFormInputView, TValue>(
            string name,
            Action<IValidationRuleBuilder> buildValidationRules = null) where TFormInputView : IFormInputView<TValue>;

        IDictionary<string, IFormInputView> Build();
    }
}