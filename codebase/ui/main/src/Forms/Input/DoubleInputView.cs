namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class DoubleInputView : FormInputView<double>
    {
        private const string Name = "DoubleInput";
        
        internal DoubleInputView(double model) : base(model) { }
        internal DoubleInputView() : this(0.0) { }
    }
}