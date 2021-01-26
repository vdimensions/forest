using System.Diagnostics.CodeAnalysis;
using Axle.Application;

namespace Forest.Web.AspNetCore
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ApplicationBuilderExtensions
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static IApplicationBuilder UseForestAspNetCore(this IApplicationBuilder app) 
            => app.ConfigureModules(m => m.Load<ForestAspNetCoreModule>());
    }
}
