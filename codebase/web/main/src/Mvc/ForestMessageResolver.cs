using System.Threading.Tasks;
using Axle.Web.AspNetCore.Mvc.ModelBinding;

namespace Forest.Web.AspNetCore.Mvc
{
    internal sealed class ForestMessageResolver : IModelResolver
    {
        private readonly ForestMessageConverter _messageConverter;

        public ForestMessageResolver(ForestMessageConverter messageConverter)
        {
            _messageConverter = messageConverter;
        }

        public async Task<object> Resolve(IMvcMetadata metadata, ModelResolutionContext next)
        {
            var path = metadata.RouteData.TryGetValue(ForestController.Message, out var msg) ? msg.ToString() : null;
            var message = path == null ? null : _messageConverter.ConvertPath(path);
            return new ForestDynamicArg(message);
        }
    }
}