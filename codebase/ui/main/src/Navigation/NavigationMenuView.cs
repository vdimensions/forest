using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.Navigation;

namespace Forest.UI.Navigation
{
    [View(Name)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal sealed class NavigationMenuView : AbstractNavigationMenuView
    {
        [ViewRegistryCallback]
        internal static void RegisterViews(IForestViewRegistry registry)
        {
            registry
                .Register<NavigationMenuItemView>()
                .Register<NavigationMenuNavigableItemView>();
        }
        
        private const string Name = "NavigationMenu";

        private static class Regions
        {
            public const string Items = "Items";
        }
        
        public override void Load()
        {
            Engine.RegisterSystemView<NavigationSystem.View>();
            base.Load();
        }

        protected override void OnNavigationTreeChanged(NavigationTree tree)
        {
            WithRegion(
                Regions.Items, 
                (itemsRegion, t) =>
                {
                    itemsRegion.Clear();
                    var topLevel = t.TopLevelNodes
                            .Select(x => new NavigationNode { Path = x, Selected = t.IsSelected(x) });
                    foreach (var item in topLevel)
                    {
                        if (item.Selected)
                        {
                            itemsRegion.ActivateView<NavigationMenuItemView, NavigationNode>(item, item.Path.Replace("/", "."));
                        }
                        else
                        {
                            itemsRegion.ActivateView<NavigationMenuNavigableItemView, NavigationNode>(item, item.Path.Replace("/", "."));
                        }
                    }
                },
                tree);
        }
    }
}
