using System.Diagnostics.CodeAnalysis;
using Axle.DependencyInjection;
using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest.Navigation
{
    [Module]
    [Requires(typeof(ForestViewRegistry))]
    internal sealed partial class NavigationModule : _ForestViewProvider
    {
        private volatile NavigationTree _navigationTree = new NavigationTree();

        [ModuleInit]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void Init(IDependencyExporter exporter) => exporter.Export(this);

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void DependencyInitialized(INavigationTreeConfigurer navigationTreeConfigurer) 
            => navigationTreeConfigurer.Configure(this);

        void _ForestViewProvider.RegisterViews(IViewRegistry registry) 
            => registry.Register<NavigationSystem.View>();
    }
}
