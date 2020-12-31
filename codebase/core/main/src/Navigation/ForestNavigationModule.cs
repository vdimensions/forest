using System;
using System.Diagnostics.CodeAnalysis;
using Axle.DependencyInjection;
using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest.Navigation
{
    [Module]
    [Requires(typeof(ForestViewRegistry))]
    internal sealed partial class ForestNavigationModule : _ForestViewProvider, INavigationManager
    {
        private volatile NavigationTree _navigationTree = new NavigationTree();

        [ModuleInit]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void Init(IDependencyExporter exporter) => exporter.Export(this);

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void DependencyInitialized(INavigationTreeConfigurer navigationTreeConfigurer) 
            => navigationTreeConfigurer.Configure(this);

        void _ForestViewProvider.RegisterViews(IForestViewRegistry registry) 
            => registry.Register<NavigationSystem.View>();

        void INavigationManager.UpdateNavigationTree(Func<INavigationTreeBuilder, INavigationTreeBuilder> configure)
        {
            var inputBuilder = new NavigationTreeBuilder(_navigationTree, NavigationTree.Root);
            var outputBuilder = configure(inputBuilder);
            var result = ((NavigationTreeBuilder) outputBuilder).Build();
            _navigationTree = result;
        }

        public NavigationTree NavigationTree => _navigationTree;
    }
}
