using Axle.Verification;
using Forest.ComponentModel;
using Forest.UI.Forms.Input;

namespace Forest.UI.Forms
{
    public static class ViewRegistryExtensions
    {
        public static IViewRegistry RegisterFormField<TInput, TValue>(this IViewRegistry registry)
            where TInput: IFormInputView<TValue>
        {
            registry.VerifyArgument(nameof(registry)).IsNotNull();
            return registry.Register<FormFieldView<TInput, TValue>>();
        }
    }
}