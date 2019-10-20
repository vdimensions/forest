using System.Collections.Generic;
using System.Threading.Tasks;
using Axle.Web.AspNetCore.Mvc.ModelBinding;

namespace Forest.Web.AspNetCore.Controllers
{
    internal sealed class ForestMessageResolver : IModelResolver
    {
        private readonly IClientViewsHelper _clientViewsHelper;

        public ForestMessageResolver(IClientViewsHelper clientViewsHelper)
        {
            _clientViewsHelper = clientViewsHelper;
        }

        public async Task<object> Resolve(IReadOnlyDictionary<string, object> routeData, ModelResolutionContext next)
        {
            var template = routeData.TryGetValue(ForestController.Template, out var tpl) ? tpl : null;
            var command = routeData.TryGetValue(ForestController.Message, out var msg) ? msg : null;
            return null;
        }
    }
}