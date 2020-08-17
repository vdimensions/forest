using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentBag<object> _listeners = new ConcurrentBag<object>();

        public ForestViewRegistry()
        {
            _viewRegistry = new ViewRegistry(_listeners);
        }
        
        [ModuleDependencyInitialized]
        internal void DependencyInitialized(_ForestViewProvider viewProvider) => viewProvider.RegisterViews(_viewRegistry);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewProvider viewProvider) => viewProvider.RegisterViews(_viewRegistry);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(_ForestViewRegistryListener viewRegistryListener) => _listeners.Add(viewRegistryListener);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewRegistryListener viewRegistryListener) => _listeners.Add(viewRegistryListener);

        public IViewDescriptor GetDescriptor(Type viewType) => _viewRegistry.GetDescriptor(viewType);
        public IViewDescriptor GetDescriptor(string viewName) => _viewRegistry.GetDescriptor(viewName);

        public void Register<T>() where T: IView => _viewRegistry.Register<T>();
        public void Register(Type viewType) => _viewRegistry.Register(viewType);

        public IEnumerable<IViewDescriptor> Descriptors => _viewRegistry.Descriptors;
    }
}
