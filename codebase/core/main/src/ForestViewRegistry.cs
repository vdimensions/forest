using System;
using System.Collections.Generic;
using Axle.Modularity;
using Forest.ComponentModel;

namespace Forest
{
    [Module]
    internal sealed class ForestViewRegistry
    {
        private readonly IViewRegistry _viewRegistry = new ViewRegistry();

        public ForestViewRegistry()
        {
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewProvider viewProvider)
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
