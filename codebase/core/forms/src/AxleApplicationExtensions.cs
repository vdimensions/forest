using System.Diagnostics.CodeAnalysis;

using Axle;
using Axle.Verification;


namespace Forest.Forms
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class AxleApplicationExtensions
    {
        public static IApplicationBuilder LoadForestForms(this IApplicationBuilder builder)
        {
            return builder.VerifyArgument(nameof(builder)).IsNotNull().Value.Load(typeof(ForestFormsModule));
        }
    }
}