using System.Diagnostics.CodeAnalysis;

using Axle;
using Axle.Verification;


namespace Forest.Forms
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class AxleApplicationExtensions
    {
        public static IApplicationBuilder UseForestForms(this IApplicationBuilder builder)
        {
            builder.VerifyArgument(nameof(builder)).IsNotNull();
            return builder.ConfigureModules(m => m.Load<ForestFormsModule>());
        }
    }
}