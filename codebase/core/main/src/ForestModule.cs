﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axle;
using Axle.DependencyInjection;
using Axle.Logging;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.Engine.Aspects;
using Forest.Security;
using Forest.StateManagement;
using Forest.Templates;
using Forest.UI;

namespace Forest
{
    [Module]
    [Requires(typeof(ForestViewRegistry))]
    [Requires(typeof(ForestTemplatesModule))]
    internal sealed class ForestModule : CollectorModule<ForestViewRegistry>, IForestEngine, IViewRegistry, IViewFactory, IForestContext, IForestExecutionAspect
    {
        [Obsolete]
        private sealed class InternalEngineContextProvider : ForestEngineContextProvider
        {
            private readonly IForestStateProvider _stateProvider;
            private readonly IPhysicalViewRenderer _physicalViewRenderer;

            public InternalEngineContextProvider(IForestStateProvider stateProvider, IPhysicalViewRenderer physicalViewRenderer)
            {
                _stateProvider = stateProvider;
                _physicalViewRenderer = physicalViewRenderer;
            }

            protected override IForestStateProvider GetForestStateProvider() => _stateProvider ?? base.GetForestStateProvider();
            protected override IPhysicalViewRenderer GetPhysicalViewRenderer() => _physicalViewRenderer ?? base.GetPhysicalViewRenderer();
        }
        private readonly ForestViewRegistry _viewRegistry;
        private readonly IViewFactory _viewFactory;
        private readonly ISecurityManager _securityManager;
        private readonly ITemplateProvider _templateProvider;
        private readonly ResourceTemplateProvider _rtp;
        private readonly ICollection<IForestExecutionAspect> _aspects;
        private readonly ILogger _logger;


        private ForestEngineContextProvider _engineProvider;
        private IForestIntegrationProvider _integrationProvider;

        public ForestModule(ForestViewRegistry viewRegistry, IContainer container, ITemplateProvider templateProvider, ResourceTemplateProvider rtp, Application app, ILogger logger) : base(viewRegistry)
        {
            _viewRegistry = viewRegistry;
            _viewFactory = new ContainerViewFactory(container.Parent ?? container, app);
            _securityManager = container.TryResolve<ISecurityManager>(out var sm) ? sm : new NoOpSecurityManager();
            _templateProvider = templateProvider;
            _rtp = rtp;
            _aspects = new List<IForestExecutionAspect>();
            _logger = logger;
        }

        [ModuleInit]
        internal void Init(ModuleExporter exporter)
        {
            foreach (var viewAssembly in _viewRegistry.Descriptors
                #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
                .Select(x => x.ViewType.Assembly)
                #else
                .Select(x => System.Reflection.IntrospectionExtensions.GetTypeInfo(x.ViewType).Assembly)
                #endif
                .Distinct())
            {
                _rtp.RegisterAssemblySource(viewAssembly);
            }

            _aspects.Add(this);
            exporter.Export(this).Export<IForestStateInspector>(new DefaultForestStateInspector());
        }

        [ModuleDependencyInitialized,Obsolete]
        internal void DependencyInitialized (IForestIntegrationProvider integrationProvider)
        {
            if (_integrationProvider != null)
            {
                throw new InvalidOperationException("Forest integration is already configured");
            }
            _integrationProvider = integrationProvider;
        }
        [ModuleDependencyInitialized]
        internal void DependencyInitialized(ForestEngineContextProvider engineProvider)
        {
            if (_engineProvider != null)
            {
                throw new InvalidOperationException("Forest engine provider is already configured");
            }
            _engineProvider = engineProvider;
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestExecutionAspect forestExecutionAspect) => _aspects.Add(forestExecutionAspect);

        [ModuleDependencyTerminated]
        internal void DependencyTerminated(IForestExecutionAspect forestExecutionAspect) => _aspects.Remove(forestExecutionAspect);

        internal ForestEngineContextProvider EngineContextProvider
        {
            get => _engineProvider ?? new InternalEngineContextProvider(_integrationProvider?.StateProvider, _integrationProvider?.Renderer);
        }

        T IForestEngine.RegisterSystemView<T>()
        {
            using (var ctx = EngineContextProvider.CreateContext(this))
            {
                return ctx.Engine.RegisterSystemView<T>();
            }
        }
        void ITreeNavigator.Navigate(string tree)
        {
            using (var ctx = EngineContextProvider.CreateContext(this))
            {
                ctx.Engine.Navigate(tree);
            }
        }
        void ITreeNavigator.Navigate<T>(string tree, T message)
        {
            using (var ctx = EngineContextProvider.CreateContext(this))
            {
                ctx.Engine.Navigate(tree, message);
            }
        }
        void IMessageDispatcher.SendMessage<T>(T msg)
        {
            using (var ctx = EngineContextProvider.CreateContext(this))
            {
                ctx.Engine.SendMessage(msg);
            }
        }
        void ICommandDispatcher.ExecuteCommand(string command, string target, object arg)
        {
            using (var ctx = EngineContextProvider.CreateContext(this))
            {
                ctx.Engine.ExecuteCommand(command, target, arg);
            }
        }

        IView IViewFactory.Resolve(IViewDescriptor descriptor) => _viewFactory.Resolve(descriptor);
        IView IViewFactory.Resolve(IViewDescriptor descriptor, object model) => _viewFactory.Resolve(descriptor, model);
        
        IViewFactory IForestContext.ViewFactory => this;
        IViewRegistry IForestContext.ViewRegistry => this;
        ISecurityManager IForestContext.SecurityManager => _securityManager;
        ITemplateProvider IForestContext.TemplateProvider => _templateProvider;
        IEnumerable<IForestExecutionAspect> IForestContext.Aspects => _aspects;

        void IForestExecutionAspect.ExecuteCommand(ExecuteCommandCutPoint cutPoint)
        {
            var sw = Stopwatch.StartNew();
            cutPoint.Proceed();
            sw.Stop();
            _logger.Trace("Forest ExecuteCommand operation took {0}ms", sw.ElapsedMilliseconds);
        }

        void IForestExecutionAspect.Navigate(NavigateCutPoint cutPoint)
        {
            var sw = Stopwatch.StartNew();
            cutPoint.Proceed();
            sw.Stop();
            _logger.Trace("Forest Navigate operation took {0}ms", sw.ElapsedMilliseconds);
        }

        void IForestExecutionAspect.SendMessage(IForestExecutionCutPoint cutPoint)
        {
            var sw = Stopwatch.StartNew();
            cutPoint.Proceed();
            sw.Stop();
            _logger.Trace("Forest SendMessage operation took {0}ms", sw.ElapsedMilliseconds);
        }

        IViewRegistry IViewRegistry.Register(Type viewType)
        {
            _viewRegistry.Register(viewType);
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            _rtp.RegisterAssemblySource(viewType.Assembly);
            #else
            _rtp.RegisterAssemblySource(System.Reflection.IntrospectionExtensions.GetTypeInfo(viewType).Assembly);
            #endif
            return this;
        }

        IViewRegistry IViewRegistry.Register<T>()
        {
            _viewRegistry.Register<T>();
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            _rtp.RegisterAssemblySource(typeof(T).Assembly);
            #else
            _rtp.RegisterAssemblySource(System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(T)).Assembly);
            #endif
            return this;
        }

        IViewDescriptor IViewRegistry.GetDescriptor(Type viewType) => _viewRegistry.GetDescriptor(viewType);
        IViewDescriptor IViewRegistry.GetDescriptor(string viewName) => _viewRegistry.GetDescriptor(viewName);
        IEnumerable<IViewDescriptor> IViewRegistry.Descriptors => _viewRegistry.Descriptors;
    }
}
