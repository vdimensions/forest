using System;
using System.Linq;
using System.Net;
using Forest.Engine;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Forest.Web.AspNetCore.Controllers
{
    [Route("api/forest")]
    public class ForestController : ControllerBase
    {
        [Serializable]
        private sealed class ForestResponse
        {
            public ForestResponse(string path, ViewNode[] views)
            {
                Path = path;
                Views = views;
            }

            public string Path { get; }
            public ViewNode[] Views { get; }
        }
        public sealed class ForestResult : ObjectResult, IClientErrorActionResult
        {
            private ForestResult(ForestResponse response, HttpStatusCode statusCode) : base(response)
            {
                StatusCode = (int) statusCode;
            }
            public ForestResult(string path, ViewNode[] views, HttpStatusCode statusCode) : this(new ForestResponse(path, views), statusCode) { }
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
        
        private string GetPath(NavigationInfo navigationInfo)
        {
            return navigationInfo.Template;
        }

        [HttpGet(GetForestViewTemplate)]
        public ActionResult GetPartial(string instanceId)
        {
            if (_clientViewsHelper.AllViews.TryGetValue(instanceId, out var view))
            {
                return new ForestResult(GetPath(_clientViewsHelper.NavigationInfo), new []{ view }, HttpStatusCode.PartialContent);
            }
            return new NotFoundResult();
        }

        [HttpGet(NavigateTemplate)]
        public ActionResult Navigate(string template)
        {
            _forest.Navigate(template);
            return new ForestResult(GetPath(_clientViewsHelper.NavigationInfo), _clientViewsHelper.AllViews.Values.ToArray(), HttpStatusCode.OK);
        }

        [HttpGet(NavigateToTemplate)]
        public ActionResult Navigate(string template, [FromRoute] IForestMessageArg arg)
        {
            _forest.Navigate(template, arg.Value);
            return new ForestResult(GetPath(_clientViewsHelper.NavigationInfo), _clientViewsHelper.AllViews.Values.ToArray(), HttpStatusCode.OK);
        }

        [HttpPatch(InvokeCommandTemplate)]
        [HttpPost(InvokeCommandTemplate)]
        public ActionResult InvokeCommand(string instanceId, string command, [FromBody] IForestCommandArg arg)
        {
            _forest.ExecuteCommand(command, instanceId, arg.Value);
            //return new ForestResult(GetPath(_clientViewsHelper.NavigationInfo), _clientViewsHelper.UpdatedViews.Values.ToArray(), HttpStatusCode.PartialContent);
            return new ForestResult(GetPath(_clientViewsHelper.NavigationInfo), _clientViewsHelper.AllViews.Values.ToArray(), HttpStatusCode.OK);
        }
    }
}