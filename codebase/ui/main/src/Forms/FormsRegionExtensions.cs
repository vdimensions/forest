using Axle.Verification;

namespace Forest.UI.Forms
{
    public static class FormsRegionExtensions
    {
        public static IFormBuilder DefineForm(this IRegion region)
        {
            region.VerifyArgument(nameof(region)).IsNotNull();
            return new FormBuilder(region);
        }
    }
}