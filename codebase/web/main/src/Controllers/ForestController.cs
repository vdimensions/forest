using Microsoft.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore.Controllers
{
    [Route("api/forest")]
    public class ForestController : ForestControllerBase
    {
        internal const string Command = "command";
        internal const string InstanceId = "instanceId";
        internal const string Message = "message";
        internal const string Template = "template";

        private const string NavigateTemplate = "{" + Template + "}";
        private const string NavigateToTemplate = "{" + Template + "}/{**" + Message + "}";
        private const string GetForestViewTemplate = "subrree/{" + InstanceId + "}";
        private const string InvokeCommandTemplate = "{" + InstanceId + "}/{" + Command + "}";
        private const string SendMessageTemplate = "{**" + Message + "}";

        public ForestController(ForestRequestExecutor forestRequestExecutor) : base(forestRequestExecutor) { }

        [HttpGet(GetForestViewTemplate)]
        public ActionResult GetPartial(string instanceId) => ForestRequestExecutor.GetPartial(instanceId);

        [HttpGet(NavigateTemplate)]
        public ActionResult Navigate(string template) => ForestRequestExecutor.Navigate(template);

        [HttpGet(NavigateToTemplate)]
        public ActionResult Navigate(string template, [FromRoute] IForestMessageArg arg) 
            => ForestRequestExecutor.Navigate(template, arg);

        [HttpPatch(InvokeCommandTemplate)]
        [HttpPost(InvokeCommandTemplate)]
        public ActionResult InvokeCommand(string instanceId, string command, [FromBody] IForestCommandArg arg) 
            => ForestRequestExecutor.ExecuteCommand(instanceId, command, arg);
    }
}