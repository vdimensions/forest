namespace Forest.Forms.Controls.Navigation
{
    public static class NavigationMenu
    {
        private const string Name = "ForestNavigation";
        public static class Item
        {
            private const string Name = "ForestNavigationItem";

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
