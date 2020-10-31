using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Navigation;
using Forest.Navigation.Messages;

namespace Forest.UI.Navigation
{
    public static partial class NavigationMenu
    {
        [Module]
        internal sealed class Module : IForestViewProvider
        {
            void IForestViewProvider.RegisterViews(IForestViewRegistry registry)
            {
                registry
                    .Register<View>()
                    .Register<Item.View>()
                    .Register<NavigableItem.View>();
            }
        }
        
        private const string Name = "NavigationMenu";

        private static class Regions
        {
            public const string Items = "Items";
        }

        internal abstract class AbstractView : LogicalView
        {
            [Subscription(NavigationSystem.Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigationTreeChanged(NavigationTreeChanged message)
            {
                OnNavigationTreeChanged(message.NavigationTree);
            }
            
            protected abstract void OnNavigationTreeChanged(NavigationTree tree);
        }

        [View(Name)]
        internal sealed class View : AbstractView
        {
            public override void Load()
            {
                Engine.RegisterSystemView<NavigationSystem.View>();
                base.Load();
            }

            protected override void OnNavigationTreeChanged(NavigationTree tree)
            {
                WithRegion(Regions.Items, itemsRegion =>
                {
                    itemsRegion.Clear();
                    var topLevel = tree.TopLevelNodes
                            .Select(x => new NavigationNode { Path = x, Selected = tree.IsSelected(x) });
                    foreach (var item in topLevel)
                    {
                        if (item.Selected)
                        {
                            itemsRegion.ActivateView<UI.Navigation.NavigationMenu.Item.View, NavigationNode>(item);
                        }
                        else
                        {
                            itemsRegion.ActivateView<UI.Navigation.NavigationMenu.NavigableItem.View, NavigationNode>(item);
                        }
                    }
                });
            }
        }
    }
}
