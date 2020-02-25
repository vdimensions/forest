namespace Forest.Forms.Controls.Navigation
{
    public static class BreadcrumbsMenu
    {
        private const string Name = "ForestBreadcrumbs";

        public static class Item
        {
            private const string Name = "ForestBreadcrumbsItem";

            [View(Name)]
            public class View : LogicalView, ISystemView
            {

            }
        }

        [View(Name)]
        public class View : LogicalView, ISystemView
        {

        }
    }
}
