namespace Forest.UI.Forms.Input
{
    public interface IFormInputView : IView
    {
        bool Validate(object value);
        
        object Value { get; }
        
        FormField Field { get; }
    }
    
    public interface IFormInputView<TValue> : IFormInputView
    {
        bool Validate(TValue value);
        
        event FormInputValueChanged<TValue> ValueChanged;
        
        new TValue Value { get; }
    }
}