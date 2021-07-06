namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableInt64InputView : FormInputView<long?>
    {
        private const string Name = "NullableInt64Input";

        internal NullableInt64InputView(long? model) : base(model) { }
        internal NullableInt64InputView(long model) : base(model) { }
        internal NullableInt64InputView() : this(null) { }
    }
}