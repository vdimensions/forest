namespace Forest.UI.Forms.Input
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class SByteInputView : FormInputView<sbyte>
    {
        private const string Name = "SByteInput";
        
        internal SByteInputView(sbyte model) : base(model) { }
        internal SByteInputView() : this(0) { }
    }
}