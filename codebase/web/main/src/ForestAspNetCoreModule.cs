using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axle.Logging;
using Axle.Modularity;
using Axle.Verification;
using Axle.Web.AspNetCore;
using Axle.Web.AspNetCore.Mvc;
using Axle.Web.AspNetCore.Mvc.ModelBinding;
using Axle.Web.AspNetCore.Session;
using Forest.Engine;
using Forest.Web.AspNetCore.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Axle.Logging.ILogger;

namespace Forest.Web.AspNetCore
{
    [Module]
    [RequiresForest]
    [RequiresAspNetSession]
    [RequiresAspNetMvc]
    internal sealed class ForestAspNetCoreModule : ISessionEventListener, IServiceConfigurer, IModelResolverProvider
    {
        private readonly IForestEngine _forestEngine;
        private readonly ForestSessionStateProvider _sessionStateProvider;
        private readonly ILogger _logger;

        public ForestAspNetCoreModule(IForestEngine forestEngine, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            _forestEngine = forestEngine;
            _sessionStateProvider = new ForestSessionStateProvider(httpContextAccessor);
            _logger = logger;
        }

        void IServiceConfigurer.Configure(IServiceCollection services)
        {
            services.AddSingleton(_forestEngine);
        }

        void ISessionEventListener.OnSessionStart(ISession session)
        {
            var sessionId = session.Id;
            _sessionStateProvider.AddOrReplace(sessionId, new ForestSessionState(), (a, b) => b);
        }

        void ISessionEventListener.OnSessionEnd(string sessionId)
        {
            if (_sessionStateProvider.TryRemove(sessionId, out _))
            {
                _logger.Trace("Deleted forest session state for session {0}", sessionId);
            }
        }

        IModelResolver IModelResolverProvider.GetModelResolver(Type modelType)
        {
            if (modelType == typeof(IForestMessageArg))
            {
                return new ForestMessageBinder();
            }
            if (modelType == typeof(IForestCommandArg))
            {
                return new ForestCommandBinder();
            }
            return null;
        }
    }

    internal sealed class ForestCommandBinder : IModelResolver
    {

        public Task<object> Resolve(IReadOnlyDictionary<string, object> routeData, ModelResolutionChain next)
        {
            var instanceId = routeData.TryGetValue(ForestController.InstanceId, out var iid) ? iid : null;
            var command = routeData.TryGetValue(ForestController.Command, out var cmd) ? cmd : null;
            return next();
        }
    }

    internal sealed class ForestMessageBinder : IModelResolver
    {
        public Task<object> Resolve(IReadOnlyDictionary<string, object> routeData, ModelResolutionChain next)
        {
            var template = routeData.TryGetValue(ForestController.Template, out var tpl) ? tpl : null;
            var command = routeData.TryGetValue(ForestController.Message, out var msg) ? msg : null;
            return next();
        }
    }
}
