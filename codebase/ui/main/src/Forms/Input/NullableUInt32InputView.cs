namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableUInt32InputView : FormInputView<uint?>
    {
        private const string Name = "NullableUInt32Input";

        internal NullableUInt32InputView(uint? model) : base(model) { }
        internal NullableUInt32InputView(uint model) : base(model) { }
        internal NullableUInt32InputView() : this(null) { }
    }
}