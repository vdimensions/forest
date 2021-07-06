namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableInt32InputView : FormInputView<int?>
    {
        private const string Name = "NullableInt32Input";

        internal NullableInt32InputView(int? model) : base(model) { }
        internal NullableInt32InputView(int model) : base(model) { }
        internal NullableInt32InputView() : this(null) { }
    }
}