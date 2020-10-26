using System.Diagnostics.CodeAnalysis;
using Axle.Application;
using Axle.Verification;

namespace Forest.UI.Navigation.Breadcrumbs
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseForestBreadcrumbs(this IApplicationBuilder builder)
        {
            builder.VerifyArgument(nameof(builder)).IsNotNull();
            return builder.ConfigureModules(m => m.Load<BreadcrumbsMenu.Module>());
        }
    }
}