﻿namespace Forest.Forms.Navigation
{
    internal static class BreadcrumbsMenuItem
    {
        private const string Name = "BreadcrumbsMenuItem";

        [View(Name)]
        internal class View : LogicalView<NavigationNode>
        {
            protected View(NavigationNode model) : base(model) { }
        }
    }
}