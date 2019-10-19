using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

using Axle.Forest.Web.Presentation.Frontend;
using Axle.Web.Http;
using Axle.Web.Http.Controllers;


namespace Axle.Forest.Web.Api.Http
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    internal sealed class ForestDecodedCommandInvokerAttribute : DelegatedParameterBindingAttribute
    {
        public ForestDecodedCommandInvokerAttribute() { }

        public override Task ExecuteBindingAsync(
            IDelegatedParameterBinding parameterBinding, 
            ModelMetadataProvider metadataProvider, 
            HttpActionContext actionContext, 
            CancellationToken cancellationToken)
        {
            var parameterBindings = actionContext.ActionDescriptor.ActionBinding.ParameterBindings;
            var commandInvokerBidning = parameterBindings.Where(pb => !pb.WillReadBody && pb.Descriptor.GetCustomAttributes<ForestEncodedCommandInvokerAttribute>().Any()).SingleOrDefault();
            var commandNameBidning = parameterBindings.Where(pb => !pb.WillReadBody && pb.Descriptor.GetCustomAttributes<ForestCommandNameAttribute>().Any()).SingleOrDefault();
            if (commandNameBidning != null && commandInvokerBidning != null)
            {
                //var controller = (ForestApiController) actionContext.ControllerContext.Controller;
                var commandInvoker = (string) actionContext.ModelState[commandInvokerBidning.Descriptor.ParameterName].Value.RawValue;
                parameterBinding.SetValue(actionContext, DecodeCommandPath(commandInvoker));
            }
            return actionContext.Request.Content.ReadAsVoidAsync();
        }

        internal static string DecodeCommandPath(string encodedPath)
        {
            return WebNodeVisitor.ConvertFromBase64(encodedPath, Encoding.UTF8);
        }
    }
}