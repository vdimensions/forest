using Axle.DependencyInjection;
using Axle.Logging;
using Axle.Modularity;
using Axle.Web.AspNetCore.Session;
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
        private readonly IForestViewRegistry _viewRegistry;
        private readonly ILogger _logger;
        private readonly ForestMessageConverter _messageConverter;

        public ForestAspNetCoreModule(IForestEngine forestEngine, IForestViewRegistry viewRegistry, ISessionReferenceProvider sessionReferenceProvider, IForestStateInspector stateInspector, ILogger logger)
        {
            _forestEngine = forestEngine;
            _viewRegistry = viewRegistry;
            _sessionStateProvider = new ForestSessionStateProvider(sessionReferenceProvider, stateInspector);
            _logger = logger;
            _messageConverter = new ForestMessageConverter();
        }

        [ModuleInit]
        internal void Init(IDependencyExporter exporter)
        {
            exporter.Export(new ForestRequestExecutor(_forestEngine, _sessionStateProvider, _messageConverter));
        }

        protected override IPhysicalViewRenderer GetPhysicalViewRenderer() => new WebApiPhysicalViewRenderer();
        
        protected override IForestStateProvider GetForestStateProvider() => this;
    }
}
