using Axle.Verification;
using Forest.ComponentModel;
using Forest.UI.Forms.Input;

namespace Forest.UI.Forms
{
    /// <summary>
    /// A static class containing extension methods that aid in registering form field views
    /// to a <see cref="IForestViewRegistry"/>.
    /// </summary>
    public static class ViewRegistryExtensions
    {
        public static IForestViewRegistry RegisterFormField<TInput, TValue>(this IForestViewRegistry registry)
            where TInput: IFormInputView<TValue>
        {
            registry.VerifyArgument(nameof(registry)).IsNotNull();
            return registry.Register<FormFieldView<TInput, TValue>>();
        }
    }
}