﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.Navigation;

namespace Forest.UI.Navigation.Breadcrumbs
{
    [View(Name)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal sealed class BreadcrumbsMenuView : AbstractNavigationMenuView
    {
        [ViewRegistryCallback]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void RegisterViews(IForestViewRegistry registry)
        {
            registry
                .Register<BreadcrumbsMenuItemView>()
                .Register<BreadcrumbsMenuNavigableItemView>();
        }
        
        private const string Name = "BreadcrumbsMenu";

        private static class Regions
        {
            public const string Items = "Items";
        }
            
        protected override void OnNavigationTreeChanged(NavigationTree tree)
        {
            WithRegion(
                Regions.Items, 
                (itemsRegion, t) =>
                {
                    itemsRegion.Clear();
                    var nodes = t.SelectedNodes.ToArray();
                    var selectedItems = nodes
                        .Select((x, i) => new NavigationNode { Path = x, Selected = true, Offset = nodes.Length - i - 1 })
                        .ToArray();
                    if (selectedItems.Length == 0)
                    {
                        return;
                    }
                    var last = selectedItems[selectedItems.Length - 1];
                    for (var i = 0; i < selectedItems.Length - 1; i++)
                    {
                        var item = selectedItems[i];
                        itemsRegion.ActivateView<BreadcrumbsMenuNavigableItemView, NavigationNode>(item, item.Path.Replace("/", "."));
                    }
                    itemsRegion.ActivateView<BreadcrumbsMenuItemView, NavigationNode>(last, last.Path.Replace("/", "."));
                },
                tree);
        }
    }
}
