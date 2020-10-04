using System;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    internal abstract class NameNode
    {
        public string Name { get; internal set; }
    }
}