using System;
using System.Collections.Generic;
using Axle.Modularity;
using Axle.Resources;
using Forest.ComponentModel;

namespace Forest
{
    [Module]
    [RequiresResources]
    internal sealed class ForestViewRegistry
    {
        private readonly IViewRegistry _viewRegistry;

        public ForestViewRegistry(ResourceManager resourceManager)
        {
            _viewRegistry = new ViewRegistry(resourceManager);
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewProvider viewProvider)
        {
            viewProvider.RegisterViews(_viewRegistry);
        }
        
        [ModuleDependencyInitialized]
        internal void DependencyInitialized(_ForestViewProvider viewProvider)
        {
            viewProvider.RegisterViews(_viewRegistry);
        }

        public IViewDescriptor GetDescriptor(Type viewType) => _viewRegistry.GetDescriptor(viewType);
        public IViewDescriptor GetDescriptor(string viewName) => _viewRegistry.GetDescriptor(viewName);

        public void Register<T>() where T: IView => _viewRegistry.Register<T>();
        public void Register(Type viewType) => _viewRegistry.Register(viewType);

        public IEnumerable<IViewDescriptor> Descriptors => _viewRegistry.Descriptors;
    }
}
