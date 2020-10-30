namespace Forest.UI.Forms.Input
{
    [View(Name)]
    public sealed class Int16InputView : FormInputView<short>
    {
        private const string Name = "Int16Input";
        
        internal Int16InputView(short model) : base(model) { }
        internal Int16InputView() : this(0) { }
    }
}