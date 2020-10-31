using Axle.Modularity;
using Forest.ComponentModel;
using Forest.UI.Forms.Input;

namespace Forest.UI.Forms
{
    [Module]
    public class FormFieldsModule : IForestViewProvider
    {
        public void RegisterViews(IForestViewRegistry registry)
        {
            registry
                .RegisterFormField<TextInputView, string>()
                .RegisterFormField<Int16InputView, short>()
                .RegisterFormField<Int32InputView, int>()
                .RegisterFormField<Int64InputView, long>()
                .RegisterFormField<SingleInputView, float>()
                .RegisterFormField<DoubleInputView, double>()
                .RegisterFormField<DecimalInputView, decimal>()
                ;
        }
    }
}