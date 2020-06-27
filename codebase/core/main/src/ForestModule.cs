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
using Forest.Navigation;
using Forest.Security;
using Forest.StateManagement;
using Forest.Templates;
using Forest.UI;

namespace Forest
{
    [Module]
    [Requires(typeof(ForestViewRegistry))]
    [Requires(typeof(ForestTemplatesModule))]
    [Requires(typeof(NavigationModule))]
    internal sealed class ForestModule : IForestEngine, IViewRegistry, IViewFactory, IForestContext, IForestCommandAdvice, IForestMessageAdvice, IForestNavigationAdvice
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
        private ForestEngineContextProvider _engineContextProvider;
        [Obsolete("Replace usages of direct module with interfaces")]
        private readonly ForestTemplatesModule _forestTemplatesModule;

        public ForestModule(
                ForestViewRegistry viewRegistry, 
                IDependencyContext dependencyContainer, 
                ITemplateProvider templateProvider,
                IDependencyContainerFactory dependencyContainerFactory, 
                ForestTemplatesModule forestTemplatesModule, 
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
            _logger = logger;

            _forestTemplatesModule = forestTemplatesModule;
        }

        [ModuleInit]
        internal void Init(IDependencyExporter exporter)
        {
            foreach (var viewAssembly in _viewRegistry.Descriptors
                #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
                .Select(x => x.ViewType.Assembly)
                #else
                .Select(x => System.Reflection.IntrospectionExtensions.GetTypeInfo(x.ViewType).Assembly)
                #endif
                .Distinct())
            {
                _forestTemplatesModule.RegisterAssemblySource(viewAssembly);
            }

            _messageAdvices.Add(this);
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
        void ITreeNavigator.Navigate(string tree)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.Navigate(tree);
            }
        }
        void ITreeNavigator.Navigate<T>(string tree, T message)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.Navigate(tree, message);
            }
        }
        void ITreeNavigator.NavigateBack()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.NavigateBack();
            }
        }
        void ITreeNavigator.NavigateUp()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViews))
            {
                ctx.Engine.NavigateUp();
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
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            _forestTemplatesModule.RegisterAssemblySource(viewType.Assembly);
            #else
            _forestTemplatesModule.RegisterAssemblySource(System.Reflection.IntrospectionExtensions.GetTypeInfo(viewType).Assembly);
            #endif
            return this;
        }

        IViewRegistry IViewRegistry.Register<T>()
        {
            _viewRegistry.Register<T>();
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            _forestTemplatesModule.RegisterAssemblySource(typeof(T).Assembly);
            #else
            _forestTemplatesModule.RegisterAssemblySource(System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(T)).Assembly);
            #endif
            return this;
        }

        IViewDescriptor IViewRegistry.GetDescriptor(Type viewType) => _viewRegistry.GetDescriptor(viewType);
        IViewDescriptor IViewRegistry.GetDescriptor(string viewName) => _viewRegistry.GetDescriptor(viewName);
        IEnumerable<IViewDescriptor> IViewRegistry.Descriptors => _viewRegistry.Descriptors;

        internal IEnumerable<IViewDescriptor> SystemViews => _viewRegistry.Descriptors.Where(x => x.IsSystemView);
    }
}
