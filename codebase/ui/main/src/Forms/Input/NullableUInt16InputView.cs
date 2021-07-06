namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableUInt16InputView : FormInputView<ushort?>
    {
        private const string Name = "NullableUInt16Input";

        internal NullableUInt16InputView(ushort? model) : base(model) { }
        internal NullableUInt16InputView(ushort model) : base(model) { }
        internal NullableUInt16InputView() : this(null) { }
    }
}