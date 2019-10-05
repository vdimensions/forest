using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
    [Requires(typeof(ForestTemplatesModule))]
    internal sealed class ForestModule : IForestEngine, IViewRegistry, IViewFactory, IForestContext, IForestExecutionAspect
    {
        private readonly IViewFactory _viewFactory;
        private readonly IViewRegistry _viewRegistry;
        private readonly ISecurityManager _securityManager;
        private readonly ITemplateProvider _templateProvider;
        private readonly ResourceTemplateProvider _resourceTemplateProvider;
        private readonly ICollection<IForestExecutionAspect> _aspects;
        private readonly ILogger _logger;


        private IForestIntegrationProvider _integrationProvider;
        private IForestContext _context;

        public ForestModule(IContainer container, ITemplateProvider templateProvider, Application app, ResourceTemplateProvider rtp, ILogger logger)
        {
            _viewFactory = new ContainerViewFactory(container.Parent ?? container, app);
            _viewRegistry = new ViewRegistry();
            _securityManager = container.TryResolve<ISecurityManager>(out var sm) ? sm : new NoOpSecurityManager();
            _templateProvider = templateProvider;
            _resourceTemplateProvider = rtp;
            _aspects = new HashSet<IForestExecutionAspect>(new ReferenceEqualityComparer<IForestExecutionAspect>());
            _logger = logger;
        }

        [ModuleInit]
        internal void Init(ModuleExporter exporter)
        {
            _aspects.Add(this);
            exporter.Export(this);
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized (IForestIntegrationProvider integrationProvider)
        {
            if (_integrationProvider != null)
            {
                throw new InvalidOperationException("Forest integration is already configured");
            }
            _integrationProvider = integrationProvider;
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestExecutionAspect forestExecutionAspect) => _aspects.Add(forestExecutionAspect);

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestViewProvider viewProvider) => viewProvider.RegisterViews(this);

        [ModuleDependencyTerminated]
        internal void DependencyTerminated(IForestExecutionAspect forestExecutionAspect) => _aspects.Remove(forestExecutionAspect);

        internal IForestEngine CreateEngine()
        {
            var pvr = _integrationProvider?.Renderer ?? new NoOpPhysicalViewRenderer();
            var sp = _integrationProvider?.StateProvider ?? new DefaultForestStateProvider();
            return new ForestEngine(this, sp, pvr);
        }

        T IForestEngine.RegisterSystemView<T>() => CreateEngine().RegisterSystemView<T>();

        void ITreeNavigator.Navigate(string tree) => CreateEngine().Navigate(tree);

        void ITreeNavigator.Navigate<T>(string tree, T message) => CreateEngine().Navigate(tree, message);

        void IMessageDispatcher.SendMessage<T>(T msg) => CreateEngine().SendMessage(msg);

        void ICommandDispatcher.ExecuteCommand(string command, string target, object arg) => CreateEngine().ExecuteCommand(command, target, arg);

        IView IViewFactory.Resolve(IViewDescriptor descriptor) => _viewFactory.Resolve(descriptor);
        IView IViewFactory.Resolve(IViewDescriptor descriptor, object model) => _viewFactory.Resolve(descriptor, model);

        IViewDescriptor IViewRegistry.GetDescriptor(Type viewType) => _viewRegistry.GetDescriptor(viewType);
        IViewDescriptor IViewRegistry.GetDescriptor(string viewName) => _viewRegistry.GetDescriptor(viewName);

        IViewRegistry IViewRegistry.Register<T>()
        {
            _resourceTemplateProvider.RegisterAssemblySource(typeof(T).GetTypeInfo().Assembly);
            _viewRegistry.Register<T>();
            return this;
        }
        IViewRegistry IViewRegistry.Register(Type viewType)
        {
            _resourceTemplateProvider.RegisterAssemblySource(viewType.GetTypeInfo().Assembly);
            _viewRegistry.Register(viewType);
            return this;
        }

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
    }
}
