using System;

namespace Forest.UI.Forms.Input
{
    public interface IFormInputView : IView
    {
        bool Validate(object value);
        
        object Value { get; }
        
        FormField Field { get; }
        
        Type ValueType { get; }
    }
    
    public interface IFormInputView<TValue> : IFormInputView
    {
        bool Validate(TValue value);
        
        new TValue Value { get; }
    }
}