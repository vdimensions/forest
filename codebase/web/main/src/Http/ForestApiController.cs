using System.Diagnostics;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;

using Axle.Forest.ComponentModel;
using Axle.Logging;

using Forest;
using Forest.Engine;


namespace Axle.Forest.Web.Api.Http
{
    public class ForestApiController : ForestController
    {
        internal const string Name = "ForestApi";
        internal const string ApiUrl = ApiUrlBase;
        internal const string ApiUrlFormat = ApiUrl + "/{0}";
        internal const string ApiCommandLinkFormat = ApiUrl + "/{0}/{1}/{2}";
        internal const string ApiCommandLinkWithArgumentFormat = ApiCommandLinkFormat + "/{3}";

        private const string ForestLayoutSessionKey = "E55E8CCE-B4D5-4285-917D-E42C30A3DF3B";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Logger _logger = Logger.For<ForestApiController>();

        private HttpSessionState Session { get { return HttpContext.Current.Session; } }

        private readonly IForestEngine forestEngine;

        public ForestApiController()
        {
            forestEngine = new ForestEngineComponent();
        }
        
        [HttpGet]
        public Response Get([ForestTemplateName] string templateName)
        {
            return ProcessRequest(templateName, false, null, null, null);
        }
        [HttpGet]
        public Response Get(
            [ForestTemplateName] string templateName,
            [ForestEncodedCommandInvoker] string invoker,
            [ForestDecodedCommandInvoker] string invokerDecoded,
            [ForestCommandName] string command)
        {
            return ProcessRequest(templateName, true, invokerDecoded, command, null);
        }
        [HttpGet]
        public Response Get(
            [ForestTemplateName] string templateName,
            [ForestEncodedCommandInvoker] string invoker,
            [ForestDecodedCommandInvoker] string invokerDecoded,
            [ForestCommandName] string command,
            [ForestCommandArg] string commandArgument,
            [ForestCommandArgument(Source = ForestCommandArgumentSource.Url)] object arg)
        {
            return ProcessRequest(templateName, true, invokerDecoded, command, arg);
        }

        [HttpPut]
        public Response Put(
            [ForestTemplateName] string templateName,
            [ForestEncodedCommandInvoker] string invoker,
            [ForestDecodedCommandInvoker] string invokerDecoded,
            [ForestCommandName] string command,
            [ForestCommandArgument(Source = ForestCommandArgumentSource.Body)] object arg)
        {
            return ProcessRequest(templateName, true, invokerDecoded, command, arg);
        }

        [HttpPost]
        public Response Post(
            [ForestTemplateName] string templateName,
            [ForestEncodedCommandInvoker] string invoker,
            [ForestDecodedCommandInvoker] string invokerDecoded,
            [ForestCommandName] string command,
            [ForestCommandArgument(Source = ForestCommandArgumentSource.Body)] object arg)
        {
            return ProcessRequest(templateName, true, invokerDecoded, command, arg);
        }

        internal Response ProcessRequest(string templateName, bool hasCommand, string invoker, string command, object arg)
        {
            var format = ResponseFormat.Diff;
            var state = Session[ForestLayoutSessionKey] as ApplicationState;
            if ((state == null) || (state.Result.Template.ID != templateName))
            {
                state = this.forestEngine.CreateState().NavigateTo(templateName);
                format = ResponseFormat.Complete;
            }

            if (hasCommand)
            {
                state = state.ExecuteCommand(invoker, command, arg);
            }

            if (state != null)
            {
                Session[ForestLayoutSessionKey] = state;
                return CreateResponse(state, format);
            }
            return null;
        }

        private static Response CreateResponse(ApplicationState state, ResponseFormat format)
        {
            return new Response(
                new ResponseHeader(
                    state.Result.Template.ID, 
                    state.Result.Template.Master, 
                    format), 
                state.RenderedView);
        }
    }
}
