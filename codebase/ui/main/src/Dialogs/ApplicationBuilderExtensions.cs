using System.Diagnostics.CodeAnalysis;
using Axle.Application;
using Axle.Verification;

namespace Forest.UI.Dialogs
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseForestDialogs(this IApplicationBuilder builder)
        {
            builder.VerifyArgument(nameof(builder)).IsNotNull();
            return builder.ConfigureModules(m => m.Load<DialogSystem.Module>());
        }
    }
}