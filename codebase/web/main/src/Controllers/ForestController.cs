using Axle.References;
using Forest.Engine;
using Microsoft.AspNetCore.Mvc;

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
    public class ForestController
    {
        internal const string Command = "command";
        internal const string InstanceId = "instanceId";
        internal const string Message = "message";
        internal const string Template = "forestTemplate";

        private const string NavigateTemplate = "{" + Template + "}";
        private const string NavigateToTemplate = "{" + Template + "}/{**" + Message + "}";
        private const string GetForestViewTemplate = "{" + InstanceId + "}";
        private const string InvokeCommandTemplate = "{" + InstanceId + "}/{" + Command + "}";
        private const string SendMessageTemplate = "{**" + Message + "}";

        private readonly IForestEngine _forest;

        public ForestController(IForestEngine forest)
        {
            _forest = forest;
        }

        [HttpGet(GetForestViewTemplate)]
        public ActionResult GetPartial(string instanceId)
        {
            return new PartialViewResult();
        }

        [HttpGet(NavigateTemplate)]
        public ActionResult Navigate(string template)
        {
            _forest.Navigate(template);
            return new OkResult();
        }

        [HttpGet(NavigateToTemplate)]
        public ActionResult Navigate(string template, [FromBody] IForestMessageArg arg)
        {
            _forest.Navigate(template, arg.Value);
            return new OkResult();
        }

        [HttpPatch(InvokeCommandTemplate)]
        [HttpPost(InvokeCommandTemplate)]
        public ActionResult InvokeCommand(string instanceId, string command, [FromBody] IForestCommandArg arg)
        {
            _forest.ExecuteCommand(command, instanceId, arg.Value);
            return new PartialViewResult();
        }

        //[HttpGet(SendMessageTemplate)]
        //[HttpPatch(SendMessageTemplate)]
        //public ActionResult SendMessage(string instanceId, string command, [FromBody] IForestMessageArg arg)
        //{
        //    _forest.SendMessage(arg.Value);
        //    return new PartialViewResult();
        //}
    }
}