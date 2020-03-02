namespace Forest.Forms.Menus.Navigation
{
    public static partial class BreadcrumbsMenu
    {
        private const string Name = "ForestBreadcrumbsMenu";

        [View(Name)]
        public class View : NavigationMenu.AbstractView<Item.View>
        {
            internal View(
                    INotifyNavigationTreeChanged notifyNavigationTreeChanged, 
                    INavigationTreeBuilder navigationTreeBuilder) 
                : base(notifyNavigationTreeChanged, navigationTreeBuilder) { }

            protected override void OnNavigationTreeChanged(NavigationTree tree)
            {
                
            }
        }
    }
}
