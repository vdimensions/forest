using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest.Forms.Menus.Navigation
{
    [Module]
    internal sealed partial class NavigationSystemModule : IForestViewProvider, INotifyNavigationTreeChanged
    {
        private volatile NavigationTree _navigationTree = new NavigationTree();

        [ModuleInit]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void Init(ModuleExporter exporter)
        {
            exporter.Export(this);
        }

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void DependencyInitialized(INavigationTreeConfigurer navigationTreeConfigurer)
        {
            var builder = new DelegatingNavigationTreeBuilder(this);
            navigationTreeConfigurer.Configure(builder);
            _navigationTree = builder.Build();
        }

        void IForestViewProvider.RegisterViews(IViewRegistry registry) =>
            registry
                .Register<NavigationMenu.View>().Register<NavigationMenu.Item.View>()
                .Register<BreadcrumbsMenu.View>().Register<BreadcrumbsMenu.Item.View>();

        public event Action<NavigationTree> NavigationTreeChanged;
    }
}
