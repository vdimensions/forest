using System;
using System.Collections.Generic;
using Axle.Modularity;
using Axle.Resources;
using Forest.ComponentModel;
using Forest.Configuration;

namespace Forest
{
    [Module]
    [RequiresResources]
    [ModuleConfigSection(typeof(ForestViewRegistryConfig), "Forest.ViewRegistry")]
    internal sealed class ForestViewRegistry
    {
        private readonly IViewRegistry _viewRegistry;

        public ForestViewRegistry(ResourceManager resourceManager) : this(resourceManager, new ForestViewRegistryConfig()) { }
        public ForestViewRegistry(ResourceManager resourceManager, ForestViewRegistryConfig config)
        {
            _viewRegistry = new ViewRegistry(resourceManager, config);
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
