namespace Forest.UI.Forms.Input
{
    [View(Name)]
    public sealed class ByteInputView : FormInputView<byte>
    {
        private const string Name = "ByteInput";
        
        internal ByteInputView(byte model) : base(model) { }
        internal ByteInputView() : this(0) { }
    }
}