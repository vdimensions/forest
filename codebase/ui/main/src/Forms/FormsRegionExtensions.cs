using Axle.Verification;

namespace Forest.UI.Forms
{
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