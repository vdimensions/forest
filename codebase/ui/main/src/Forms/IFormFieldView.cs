using Forest.UI.Forms.Input;

namespace Forest.UI.Forms
{
    internal interface IFormFieldView : IView<FormField>
    {
        IFormInputView FormInputView { get; }
    }
}