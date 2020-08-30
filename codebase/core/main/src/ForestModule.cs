using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axle.DependencyInjection;
using Axle.Logging;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Dom;
using Forest.Engine;
using Forest.Engine.Aspects;
using Forest.Globalization;
using Forest.Navigation;
using Forest.Security;
using Forest.StateManagement;
using Forest.Templates;

namespace Forest
{
    [Module]
    [Requires(typeof(ForestViewRegistry))]
    [Requires(typeof(ForestTemplatesModule))]
    [Requires(typeof(NavigationModule))]
    [Requires(typeof(ForestGlobalizationModule))]
    internal sealed class ForestModule : 
        IForestEngine, 
        IViewRegistry, 
        IViewFactory, 
        IForestContext, 
        IForestCommandAdvice, 
        IForestMessageAdvice, 
        IForestNavigationAdvice
    {
        private readonly ForestViewRegistry _viewRegistry;
        private readonly IViewFactory _viewFactory;
        private readonly ISecurityManager _securityManager;
        private readonly ITemplateProvider _templateProvider;
        private readonly IForestDomManager _domManager;
        private readonly ICollection<IForestCommandAdvice> _commandAdvices;
        private readonly ICollection<IForestMessageAdvice> _messageAdvices;
        private readonly ICollection<IForestNavigationAdvice> _navigationAdvices;
        private readonly ILogger _logger;
        private readonly ForestGlobalizationModule _forestGlobalizationModule;
        private ForestEngineContextProvider _engineContextProvider;

        public ForestModule(
                ForestViewRegistry viewRegistry, 
                IDependencyContext dependencyContainer, 
                ITemplateProvider templateProvider,
                IDependencyContainerFactory dependencyContainerFactory, 
                ForestGlobalizationModule globalizationModule,
                ILogger logger) 
        {
            _viewRegistry = viewRegistry;
            _viewFactory = new ContainerViewFactory(dependencyContainer.Parent ?? dependencyContainer, dependencyContainerFactory);
            _securityManager = dependencyContainer.TryResolve<ISecurityManager>(out var sm) ? sm : new NoOpSecurityManager();
            _templateProvider = templateProvider;
            _domManager = new ForestDomManager(this);
            _messageAdvices = new List<IForestMessageAdvice>();
            _commandAdvices = new List<IForestCommandAdvice>();
            _navigationAdvices = new List<IForestNavigationAdvice>();
            _forestGlobalizationModule = globalizationModule;
            _logger = logger;
        }

        [ModuleInit]
        internal void Init(IDependencyExporter exporter)
        {
            _messageAdvices.Add(this);
            _commandAdvices.Add(this);
            _navigationAdvices.Add(this);
            exporter.Export(this).Export<IForestStateInspector>(new DefaultForestStateInspector());
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(ForestEngineContextProvider engineContextProvider)
        {
            if (_engineContextProvider != null)
            {
                throw new InvalidOperationException("Forest engine provider is already configured");
            }
            _engineContextProvider = engineContextProvider;
        }

        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestCommandAdvice forestCommandAdvice) => _commandAdvices.Add(forestCommandAdvice);
        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestMessageAdvice forestMessageAdvice) => _messageAdvices.Add(forestMessageAdvice);
        [ModuleDependencyInitialized]
        internal void DependencyInitialized(IForestNavigationAdvice forestNavigationAdvice) => _navigationAdvices.Add(forestNavigationAdvice);

        [ModuleDependencyTerminated]
        internal void DependencyTerminated(IForestCommandAdvice forestCommandAdvice) => _commandAdvices.Remove(forestCommandAdvice);
        [ModuleDependencyTerminated]
        internal void DependencyTerminated(IForestMessageAdvice forestMessageAdvice) => _messageAdvices.Remove(forestMessageAdvice);
        [ModuleDependencyTerminated]
        internal void DependencyTerminated(IForestNavigationAdvice forestNavigationAdvice) => _navigationAdvices.Remove(forestNavigationAdvice);

        internal ForestEngineContextProvider EngineContextProvider => _engineContextProvider;

        T IForestEngine.RegisterSystemView<T>()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                return ctx.Engine.RegisterSystemView<T>();
            }
        }
        IView IForestEngine.RegisterSystemView(Type viewType)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                return ctx.Engine.RegisterSystemView(viewType);
            }
        }
        void ITreeNavigator.Navigate(string path)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.Navigate(path);
            }
        }
        void ITreeNavigator.Navigate<T>(string path, T state)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.Navigate(path, state);
            }
        }
        void ITreeNavigator.NavigateBack()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.NavigateBack();
            }
        }
        void ITreeNavigator.NavigateBack(int offset)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.NavigateBack(offset);
            }
        }
        void ITreeNavigator.NavigateUp()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.NavigateUp();
            }
        }
        void ITreeNavigator.NavigateUp(int offset)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.NavigateUp(offset);
            }
        }
        void IMessageDispatcher.SendMessage<T>(T msg)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.SendMessage(msg);
            }
        }
        void ICommandDispatcher.ExecuteCommand(string command, string target, object arg)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
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
        IForestDomManager IForestContext.DomManager => _domManager;
        IDomProcessor IForestContext.GlobalizationDomProcessor => _forestGlobalizationModule;
        IEnumerable<IForestCommandAdvice> IForestContext.CommandAdvices => _commandAdvices;
        IEnumerable<IForestMessageAdvice> IForestContext.MessageAdvices => _messageAdvices;
        IEnumerable<IForestNavigationAdvice> IForestContext.NavigationAdvices => _navigationAdvices;

        void IForestCommandAdvice.ExecuteCommand(IExecuteCommandPointcut pointcut)
        {
            var sw = Stopwatch.StartNew();
            pointcut.Proceed();
            sw.Stop();
            _logger.Trace("Forest ExecuteCommand operation took {0}ms", sw.ElapsedMilliseconds.ToString());
        }

        void IForestMessageAdvice.SendMessage(ISendMessagePointcut pointcut)
        {
            var sw = Stopwatch.StartNew();
            pointcut.Proceed();
            sw.Stop();
            _logger.Trace("Forest SendMessage operation took {0}ms", sw.ElapsedMilliseconds.ToString());
        }

        void IForestNavigationAdvice.Navigate(INavigatePointcut pointcut)
        {
            var sw = Stopwatch.StartNew();
            pointcut.Proceed();
            sw.Stop();
            _logger.Trace("Forest Navigate operation took {0}ms", sw.ElapsedMilliseconds.ToString());
        }

        IViewRegistry IViewRegistry.Register(Type viewType)
        {
            _viewRegistry.Register(viewType);
            return this;
        }

        IViewRegistry IViewRegistry.Register<T>()
        {
            _viewRegistry.Register<T>();
            return this;
        }

        IViewDescriptor IViewRegistry.GetDescriptor(Type viewType) => _viewRegistry.GetDescriptor(viewType);
        IViewDescriptor IViewRegistry.GetDescriptor(string viewName) => _viewRegistry.GetDescriptor(viewName);
        IEnumerable<IViewDescriptor> IViewRegistry.Descriptors => _viewRegistry.Descriptors;

        internal IEnumerable<IViewDescriptor> SystemViews => _viewRegistry.Descriptors.Where(x => x.IsSystemView);
    }
}
