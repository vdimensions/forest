namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class UInt16InputView : FormInputView<ushort>
    {
        private const string Name = "UInt16Input";
        
        internal UInt16InputView(ushort model) : base(model) { }
        internal UInt16InputView() : this(0) { }
    }
}