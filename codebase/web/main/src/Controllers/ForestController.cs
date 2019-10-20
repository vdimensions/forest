using System.Linq;
using System.Net;
using Forest.Engine;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Forest.Web.AspNetCore.Controllers
{
    public interface IForestCommandArg
    {
        object Value { get; }
    }
    public interface IForestMessageArg
    {
        object Value { get; }
    }

    [Route("api/forest")]
    public class ForestController : ControllerBase
    {
        public sealed class ForestResult : ObjectResult, IClientErrorActionResult
        {
            public ForestResult(ViewNode[] views, HttpStatusCode statusCode) : base(views)
            {
                StatusCode = (int) statusCode;
            }
        }

        internal const string Command = "command";
        internal const string InstanceId = "instanceId";
        internal const string Message = "message";
        internal const string Template = "template";

        private const string NavigateTemplate = "{" + Template + "}";
        private const string NavigateToTemplate = "{" + Template + "}/{**" + Message + "}";
        private const string GetForestViewTemplate = "subrree/{" + InstanceId + "}";
        private const string InvokeCommandTemplate = "{" + InstanceId + "}/{" + Command + "}";
        private const string SendMessageTemplate = "{**" + Message + "}";

        private readonly IForestEngine _forest;
        private readonly IClientViewsHelper _clientViewsHelper;

        public ForestController(IForestEngine forest, IClientViewsHelper clientViewsHelper)
        {
            _forest = forest;
            _clientViewsHelper = clientViewsHelper;
        }

        [HttpGet(GetForestViewTemplate)]
        public ActionResult GetPartial(string instanceId)
        {
            return new ForestResult(_clientViewsHelper.UpdatedViews.Values.ToArray(), HttpStatusCode.PartialContent);
        }

        [HttpGet(NavigateTemplate)]
        public ActionResult Navigate(string template)
        {
            _forest.Navigate(template);
            return new ForestResult(_clientViewsHelper.AllViews.Values.ToArray(), HttpStatusCode.OK);
        }

        [HttpGet(NavigateToTemplate)]
        public ActionResult Navigate(string template, [FromBody] IForestMessageArg arg)
        {
            _forest.Navigate(template, arg.Value);
            return new ForestResult(_clientViewsHelper.AllViews.Values.ToArray(), HttpStatusCode.OK);
        }

        [HttpPatch(InvokeCommandTemplate)]
        [HttpPost(InvokeCommandTemplate)]
        public ActionResult InvokeCommand(string instanceId, string command, [FromBody] IForestCommandArg arg)
        {
            _forest.ExecuteCommand(command, instanceId, arg.Value);
            return new ForestResult(_clientViewsHelper.UpdatedViews.Values.ToArray(), HttpStatusCode.PartialContent);
        }
    }
}