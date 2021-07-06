using System;

namespace Forest.UI.Forms.Input
{
    public interface IFormInputView : IView
    {
        bool Validate(FormField field, object value);
        
        object Value { get; }
        
        Type ValueType { get; }
    }
    
    public interface IFormInputView<TValue> : IFormInputView
    {
        bool Validate(FormField field, TValue value);
        
        new TValue Value { get; }
    }
}