using System.Threading.Tasks;
using Axle.Web.AspNetCore.Mvc.ModelBinding;

namespace Forest.Web.AspNetCore.Mvc
{
    internal sealed class ForestCommandResolver : IModelResolver
    {
        private readonly IClientViewsHelper _clientViewsHelper;

        public ForestCommandResolver(IClientViewsHelper clientViewsHelper)
        {
            _clientViewsHelper = clientViewsHelper;
        }

        public async Task<object> Resolve(IMvcMetadata metadata, ModelResolutionContext next)
        {
            var instanceId = metadata.RouteData.TryGetValue(ForestController.InstanceId, out var iid) ? (string) iid : null;
            var command = metadata.RouteData.TryGetValue(ForestController.Command, out var cmd) ? (string) cmd : null;
            if (command != null 
                && _clientViewsHelper.TryGetViewDescriptor(instanceId, out var descriptor) 
                && descriptor.Commands.TryGetValue(command, out var cd))
            {
                if (cd.ArgumentType == null || cd.ArgumentType == typeof(void))
                {
                    return new ForestVoidArg();
                }
                var commandArg = await next.Resolve(cd.ArgumentType);
                return new ForestDynamicArg(commandArg);
            }
            return null;
        }
    }
}