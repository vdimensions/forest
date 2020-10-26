using System.Diagnostics.CodeAnalysis;
using Axle.Application;
using Axle.Verification;

namespace Forest.UI.Navigation
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseForestNavigationMenu(this IApplicationBuilder builder)
        {
            builder.VerifyArgument(nameof(builder)).IsNotNull();
            return builder.ConfigureModules(m => m.Load<NavigationMenu.Module>());
        }
    }
}