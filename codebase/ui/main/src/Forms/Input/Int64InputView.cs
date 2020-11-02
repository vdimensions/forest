namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class Int64InputView : FormInputView<long>
    {
        private const string Name = "Int64Input";

        internal Int64InputView(long model) : base(model) { }
        internal Int64InputView() : this(0L) { }
    }
}