namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableInt16InputView : FormInputView<short?>
    {
        private const string Name = "NullableInt16Input";

        internal NullableInt16InputView(short? model) : base(model) { }
        internal NullableInt16InputView(short model) : base(model) { }
        internal NullableInt16InputView() : this(null) { }
    }
}