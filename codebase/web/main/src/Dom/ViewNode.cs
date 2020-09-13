using System;
using System.Collections.Generic;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    public class ViewNode : NameNode
    {
        public string ID { get; internal set; }
        public object Model { get; internal set; }
        public IDictionary<string, CommandNode> Commands { get; internal set; }
        public IDictionary<string, string[]> Regions { get; internal set; }
    }
}