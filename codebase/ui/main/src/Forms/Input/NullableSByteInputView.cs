namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class NullableSByteInputView : FormInputView<sbyte?>
    {
        private const string Name = "NullableSByteInput";

        internal NullableSByteInputView(sbyte? model) : base(model) { }
        internal NullableSByteInputView(sbyte model) : base(model) { }
        internal NullableSByteInputView() : this(null) { }
    }
}