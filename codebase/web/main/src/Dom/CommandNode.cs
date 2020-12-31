using System;
using Forest.Dom;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    internal sealed class CommandNode : ICommandModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
        public string Description { get; set; }
    }
}
