namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class UInt32InputView : FormInputView<uint>
    {
        private const string Name = "UInt32Input";
        
        internal UInt32InputView(uint model) : base(model) { }
        internal UInt32InputView() : this(0) { }
    }
}