namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class TextInputView : FormInputView<string>
    {
        private const string Name = "TextInput";
        
        internal TextInputView(string model) : base(model) { }
        internal TextInputView() : this(string.Empty) { }
    }
}