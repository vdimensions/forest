using System.Diagnostics.CodeAnalysis;
using Axle.DependencyInjection;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Forms.Navigation.Breadcrumbs;

namespace Forest.Forms.Navigation
{
    [Module]
    internal sealed partial class NavigationSystemModule : IForestViewProvider
    {
        private volatile NavigationTree _navigationTree = new NavigationTree();

        [ModuleInit]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void Init(IDependencyExporter exporter)
        {
            exporter.Export(this);
        }

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void DependencyInitialized(INavigationTreeConfigurer navigationTreeConfigurer) => navigationTreeConfigurer.Configure(this);

        void IForestViewProvider.RegisterViews(IViewRegistry registry) 
            => registry
                .Register<NavigationSystem.View>()
                .Register<NavigationMenu.View>()
                    .Register<NavigationMenu.Item.View>()
                    .Register<NavigationMenu.NavigableItem.View>()
                .Register<BreadcrumbsMenu.View>()
                    .Register<BreadcrumbsMenuItem.View>()
                    .Register<BreadcrumbsMenuNavigableItem.View>();
    }
}
