using System.Collections.Generic;
using System.Threading.Tasks;
using Axle.Web.AspNetCore.Mvc.ModelBinding;

namespace Forest.Web.AspNetCore.Controllers
{
    internal sealed class ForestDynamicArg : IForestCommandArg, IForestMessageArg
    {
        public ForestDynamicArg(object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
    internal sealed class ForestVoidArg : IForestCommandArg, IForestMessageArg
    {
        public object Value => null;
    }
    internal sealed class ForestCommandResolver : IModelResolver
    {
        private readonly IClientViewsHelper _clientViewsHelper;

        public ForestCommandResolver(IClientViewsHelper clientViewsHelper)
        {
            _clientViewsHelper = clientViewsHelper;
        }

        public async Task<object> Resolve(IReadOnlyDictionary<string, object> routeData, ModelResolutionContext next)
        {
            var instanceId = routeData.TryGetValue(ForestController.InstanceId, out var iid) ? (string) iid : null;
            var command = routeData.TryGetValue(ForestController.Command, out var cmd) ? (string) cmd : null;
            if (_clientViewsHelper.TryGetDescriptor(instanceId, out var descriptor) && descriptor.Commands.TryGetValue(command, out var cd))
            {
                if (cd.ArgumentType == null || cd.ArgumentType == typeof(void))
                {
                    return new ForestVoidArg();
                }
                var commandArg = next.Resolve(cd.ArgumentType);
                return new ForestDynamicArg(commandArg);
            }
            return null;
        }
    }
}