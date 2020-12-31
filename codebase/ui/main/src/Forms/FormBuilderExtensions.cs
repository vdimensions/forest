using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Verification;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    /// <summary>
    /// A static class containing extension methods for the <see cref="IFormBuilder"/> interface.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class FormBuilderExtensions
    {
        public static IFormBuilder AddField<TFormInput, TValue>(
            this IFormBuilder builder, 
            string name, 
            Action<IValidationRuleBuilder> b) where TFormInput: IFormInputView<TValue>
        {
            builder.VerifyArgument(nameof(builder)).IsNotNull();
            return builder.AddField<TFormInput, TValue>(name, default(TValue), b);
        }
    }
}