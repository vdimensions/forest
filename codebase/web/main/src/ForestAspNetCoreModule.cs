using Axle.DependencyInjection;
using Axle.Logging;
using Axle.Modularity;
using Axle.Web.AspNetCore.Mvc.ModelBinding;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.StateManagement;
using Forest.UI;
using Forest.Web.AspNetCore.Dom;
using Forest.Web.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Forest.Web.AspNetCore
{
    [Module]
    [RequiresForest]
    internal sealed partial class ForestAspNetCoreModule : ForestEngineContextProvider
    {
        private readonly IForestEngine _forestEngine;
        private readonly IViewRegistry _viewRegistry;
        private readonly ILogger _logger;

        public ForestAspNetCoreModule(IForestEngine forestEngine, IViewRegistry viewRegistry, IHttpContextAccessor httpContextAccessor, IForestStateInspector stateInspector, ILogger logger)
        {
            _forestEngine = forestEngine;
            _viewRegistry = viewRegistry;
            _sessionStateProvider = new ForestSessionStateProvider(httpContextAccessor, stateInspector);
            _logger = logger;
        }

        [ModuleInit]
        internal void Init(IDependencyExporter exporter)
        {
            exporter.Export(new ForestRequestExecutor(_forestEngine, _sessionStateProvider));
        }

        public void RegisterTypes(IModelTypeRegistry registry)
        {
            foreach (var descriptor in _viewRegistry.Descriptors)
            {
                registry.Register(descriptor.ModelType);
                foreach (var eventDescriptor in descriptor.Events)
                {
                    if (!string.IsNullOrEmpty(eventDescriptor.Topic))
                    {
                        continue;
                    }
                    registry.Register(eventDescriptor.MessageType);
                }
                foreach (var commandDescriptor in descriptor.Commands.Values)
                {
                    if (commandDescriptor.ArgumentType == null)
                    {
                        continue;
                    }
                    registry.Register(commandDescriptor.ArgumentType);
                }
            }
        }

        protected override IPhysicalViewRenderer GetPhysicalViewRenderer() => new WebApiPhysicalViewRenderer(_sessionStateProvider);
        
        protected override IForestStateProvider GetForestStateProvider() => this;
    }
}
