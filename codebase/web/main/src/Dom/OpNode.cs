using System;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    public abstract class OpNode : NameNode
    {
        public Uri Href { get; }
        public string ToolTip { get; }
        public string Description { get; }
    }
}