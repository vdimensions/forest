using Axle.Web.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

namespace Forest.Web.AspNetCore
{
    internal sealed class ForestSessionStateProvider : SessionScoped<ForestSessionState>
    {
        public ForestSessionStateProvider(IHttpContextAccessor accessor) : base(accessor) { }
    }
}