﻿using Axle.DependencyInjection;
using Axle.Logging;
using Axle.Modularity;
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
        private readonly ForestMessageConverter _messageConverter;

        public ForestAspNetCoreModule(IForestEngine forestEngine, IViewRegistry viewRegistry, IHttpContextAccessor httpContextAccessor, IForestStateInspector stateInspector, ILogger logger)
        {
            _forestEngine = forestEngine;
            _viewRegistry = viewRegistry;
            _sessionStateProvider = new ForestSessionStateProvider(httpContextAccessor, stateInspector);
            _logger = logger;
            _messageConverter = new ForestMessageConverter();
        }

        [ModuleInit]
        internal void Init(IDependencyExporter exporter)
        {
            exporter.Export(new ForestRequestExecutor(_forestEngine, _sessionStateProvider, _messageConverter));
        }

        protected override IPhysicalViewRenderer GetPhysicalViewRenderer() => new WebApiPhysicalViewRenderer(_sessionStateProvider);
        
        protected override IForestStateProvider GetForestStateProvider() => this;
    }
}
