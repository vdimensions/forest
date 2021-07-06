namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableByteInputView : FormInputView<byte?>
    {
        private const string Name = "NullableByteInput";

        internal NullableByteInputView(byte? model) : base(model) { }
        internal NullableByteInputView(byte model) : base(model) { }
        internal NullableByteInputView() : this(null) { }
    }
}