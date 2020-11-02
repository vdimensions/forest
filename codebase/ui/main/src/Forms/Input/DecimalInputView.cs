namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class DecimalInputView : FormInputView<decimal>
    {
        private const string Name = "DecimalInput";
        
        internal DecimalInputView(decimal model) : base(model) { }
        internal DecimalInputView() : this(0m) { }
    }
}