namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class Int32InputView : FormInputView<int>
    {
        private const string Name = "Int32Input";
        
        internal Int32InputView(int model) : base(model) { }
        internal Int32InputView() : this(0) { }
    }
}