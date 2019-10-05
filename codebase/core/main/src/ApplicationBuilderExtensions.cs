using System.Diagnostics.CodeAnalysis;
using Axle;
using Axle.Verification;

namespace Forest
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseForest(this IApplicationBuilder app)
        {
            app.VerifyArgument(nameof(app)).IsNotNull();
            return app.ConfigureModules(m => m.Load<ForestModule>());
        }
    }
}