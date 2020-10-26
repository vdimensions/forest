using System.Diagnostics.CodeAnalysis;

namespace Forest.UI.Forms
{
    /// <summary>
    /// A view that is used to represent validation messages.
    /// </summary>
    [View(Name)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class FormFieldValidationMessage : LogicalView<string>
    {
        private const string Name = "FormFieldValidationMessage";

        internal FormFieldValidationMessage(string model) : base(model) { }
    }
}