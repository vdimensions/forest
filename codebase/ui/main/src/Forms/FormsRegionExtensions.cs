using System.Diagnostics.CodeAnalysis;
using Axle.Verification;

namespace Forest.UI.Forms
{
    /// <summary>
    /// A static class containing extension methods for the <see cref="IRegion"/> interface
    /// that add support for working with <see cref="IFormBuilder">form builders</see>  
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class FormsRegionExtensions
    {
        public static IFormBuilder DefineForm(this IRegion region, string formName)
        {
            region.VerifyArgument(nameof(region)).IsNotNull();
            formName.VerifyArgument(nameof(formName)).IsNotNullOrEmpty();
            return new FormBuilder(region, formName);
        }
    }
}