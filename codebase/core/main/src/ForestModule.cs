using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axle.DependencyInjection;
using Axle.Logging;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.Engine.Aspects;
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
    internal sealed class ForestModule : IForestEngine, IViewRegistry, IViewFactory, IForestContext, IForestExecutionAspect
    {
        private readonly ForestViewRegistry _viewRegistry;
        private readonly IViewFactory _viewFactory;
        private readonly ISecurityManager _securityManager;
        private readonly ITemplateProvider _templateProvider;
        private readonly ICollection<IForestExecutionAspect> _aspects;
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
            _aspects = new List<IForestExecutionAspect>();
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

            _aspects.Add(this);
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
        internal void DependencyInitialized(IForestExecutionAspect forestExecutionAspect) => _aspects.Add(forestExecutionAspect);

        [ModuleDependencyTerminated]
        internal void DependencyTerminated(IForestExecutionAspect forestExecutionAspect) => _aspects.Remove(forestExecutionAspect);

        internal ForestEngineContextProvider EngineContextProvider => _engineContextProvider;

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
            _logger.Trace("Forest ExecuteCommand operation took {0}ms", sw.ElapsedMilliseconds.ToString());
        }

        void IForestExecutionAspect.Navigate(NavigateCutPoint cutPoint)
        {
            var sw = Stopwatch.StartNew();
            cutPoint.Proceed();
            sw.Stop();
            _logger.Trace("Forest Navigate operation took {0}ms", sw.ElapsedMilliseconds.ToString());
        }

        void IForestExecutionAspect.SendMessage(IForestExecutionCutPoint cutPoint)
        {
            var sw = Stopwatch.StartNew();
            cutPoint.Proceed();
            sw.Stop();
            _logger.Trace("Forest SendMessage operation took {0}ms", sw.ElapsedMilliseconds.ToString());
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
    }
}
