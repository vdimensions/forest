namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableDoubleInputView : FormInputView<double?>
    {
        private const string Name = "NullableDoubleInput";

        internal NullableDoubleInputView(double? model) : base(model) { }
        internal NullableDoubleInputView(double model) : base(model) { }
        internal NullableDoubleInputView() : base(null) { }
    }
}