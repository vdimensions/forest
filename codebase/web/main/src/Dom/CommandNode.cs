﻿using System;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    public sealed class CommandNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ToolTip { get; set; }
        public string Description { get; set; }
    }
}
