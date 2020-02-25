using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest.Forms.Controls.Navigation
{
    [Module]
    internal sealed class ForestNavigationSystemModule : IForestViewProvider, INavigationBuilder
    {
        private readonly NavigationTree _navigationTree = new NavigationTree();

        [ModuleInit]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void Init(ModuleExporter exporter)
        {
            exporter.Export(_navigationTree);
        }

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void DependencyInitialized(INavigationConfigurer navigationConfigurer) => 
            navigationConfigurer.Configure(this);

        public INavigationTreeBuilder RegisterNavigationTree(string navigationItem) => 
            new NavigationTreeBuilder(NavigationTree.Root, _navigationTree).RegisterNavigationTree(navigationItem);

        void IForestViewProvider.RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<NavigationSystem.View>()
                .Register<NavigationMenu.View>().Register<NavigationMenu.Item.View>()
                .Register<BreadcrumbsMenu.View>().Register<BreadcrumbsMenu.Item.View>()
                ;
        }
    }
}
