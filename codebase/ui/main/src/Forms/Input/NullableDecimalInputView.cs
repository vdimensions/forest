namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableDecimalInputView : FormInputView<decimal?>
    {
        private const string Name = "NullableDecimalInput";

        internal NullableDecimalInputView(decimal? model) : base(model) { }
        internal NullableDecimalInputView(decimal model) : base(model) { }
        internal NullableDecimalInputView() : base(null) { }
    }
}