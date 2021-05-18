namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class UInt64InputView : FormInputView<ulong>
    {
        private const string Name = "UInt64Input";

        internal UInt64InputView(ulong model) : base(model) { }
        internal UInt64InputView() : this(0L) { }
    }
}