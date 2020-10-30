namespace Forest.UI.Forms.Input
{
    [View(Name)]
    public sealed class SingleInputView : FormInputView<float>
    {
        private const string Name = "SingleInput";
        
        internal SingleInputView(float model) : base(model) { }
        internal SingleInputView() : this(0f) { }
    }
}