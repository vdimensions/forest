using System;
using System.Collections.Generic;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    internal class ViewNode : NameNode
    {
        public string ID { get; internal set; }
        public object Model { get; internal set; }
        public IReadOnlyDictionary<string, CommandNode> Commands { get; internal set; }
        public IReadOnlyDictionary<string, string[]> Regions { get; internal set; }
    }
}