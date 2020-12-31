using System;
using System.Collections.Generic;
using Axle.Modularity;
using Axle.Resources;
using Forest.ComponentModel;

namespace Forest
{
    [Module]
    [RequiresResources]
    internal sealed class ForestViewRegistry : IForestViewRegistry
    {
        private readonly ViewRegistry _viewRegistry;

        public ForestViewRegistry()
        {
            _viewRegistry = new ViewRegistry();
        }
        
        [ModuleDependencyInitialized]
        internal void DependencyInitialized(_ForestViewProvider viewProvider) => viewProvider.RegisterViews(_viewRegistry);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewProvider viewProvider) => viewProvider.RegisterViews(_viewRegistry);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(_ForestViewRegistryListener viewRegistryListener) => _viewRegistry.AddListener(viewRegistryListener);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewRegistryListener viewRegistryListener) => _viewRegistry.AddListener(viewRegistryListener);

        public IForestViewDescriptor Describe(Type viewType) => _viewRegistry.Describe(viewType);
        public IForestViewDescriptor Describe(string viewName) => _viewRegistry.Describe(viewName);

        public IForestViewRegistry Register<T>() where T: IView => _viewRegistry.Register<T>();
        public IForestViewRegistry Register(Type viewType) => _viewRegistry.Register(viewType);

        public IEnumerable<IForestViewDescriptor> ViewDescriptors => _viewRegistry.ViewDescriptors;
    }
}
