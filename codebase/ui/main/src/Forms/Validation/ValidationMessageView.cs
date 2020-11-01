using System.Diagnostics.CodeAnalysis;

namespace Forest.UI.Forms.Validation
{
    /// <summary>
    /// A view that is used to represent validation messages.
    /// </summary>
    [View(Name)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class ValidationMessageView : LogicalView<string>
    {
        private const string Name = "ValidationMessage";

        internal ValidationMessageView(string model) : base(model) { }
    }
}