namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableUInt64InputView : FormInputView<ulong?>
    {
        private const string Name = "NullableUInt64Input";

        internal NullableUInt64InputView(ulong? model) : base(model) { }
        internal NullableUInt64InputView(ulong model) : base(model) { }
        internal NullableUInt64InputView() : this(null) { }
    }
}