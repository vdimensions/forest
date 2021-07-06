namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableSingleInputView : FormInputView<float?>
    {
        private const string Name = "NullableSingleInput";

        internal NullableSingleInputView(float? model) : base(model) { }
        internal NullableSingleInputView(float model) : base(model) { }
        internal NullableSingleInputView() : base(null) { }
    }
}