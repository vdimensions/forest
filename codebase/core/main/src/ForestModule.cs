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
using Forest.Engine.Instructions;
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
    [Requires(typeof(ForestNavigationModule))]
    [Requires(typeof(ForestGlobalizationModule))]
    [Requires(typeof(ForestSecurityModule))]
    internal sealed class ForestModule :
        IForestEngine, 
        IForestViewFactory, 
        IForestContext, 
        IForestCommandAdvice, 
        IForestMessageAdvice, 
        IForestNavigationAdvice
    {
        private readonly IForestViewRegistry _viewRegistry;
        private readonly IForestViewFactory _viewFactory;
        private readonly IForestSecurityManager _securityManager;
        private readonly IForestSecurityExceptionHandler _securityExceptionHandler;
        private readonly ITemplateProvider _templateProvider;
        private readonly IForestDomManager _domManager;
        private readonly ICollection<IForestCommandAdvice> _commandAdvices;
        private readonly ICollection<IForestMessageAdvice> _messageAdvices;
        private readonly ICollection<IForestNavigationAdvice> _navigationAdvices;
        private readonly ILogger _logger;
        private readonly ForestGlobalizationModule _forestGlobalizationModule;
        private ForestEngineContextProvider _engineContextProvider;

        public ForestModule(
                IForestViewRegistry viewRegistry, 
                IDependencyContext dependencyContainer, 
                ITemplateProvider templateProvider,
                IForestSecurityManager securityManager,
                IForestSecurityExceptionHandler securityExceptionHandler,
                IDependencyContainerFactory dependencyContainerFactory, 
                ForestGlobalizationModule globalizationModule,
                ILogger logger)
        {
            _viewRegistry = viewRegistry;
            _viewFactory = new ContainerViewFactory(dependencyContainer.Parent ?? dependencyContainer, dependencyContainerFactory);
            _securityManager = securityManager;
            _securityExceptionHandler = securityExceptionHandler;
            _templateProvider = templateProvider;
            _domManager = new ForestDomManager(this, logger);
            _messageAdvices = new List<IForestMessageAdvice>();
            _commandAdvices = new List<IForestCommandAdvice>();
            _navigationAdvices = new List<IForestNavigationAdvice>();
            _forestGlobalizationModule = globalizationModule;
            _logger = logger;
        }

        [ModuleInit]
        internal void Initialize(IDependencyExporter exporter)
        {
            _messageAdvices.Add(this);
            _commandAdvices.Add(this);
            _navigationAdvices.Add(this);
            exporter
                .Export(this)
                .Export(_viewRegistry)
                .Export(_securityExceptionHandler)
                .Export<IForestStateInspector>(new DefaultForestStateInspector())
                ;
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
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                return ctx.Engine.RegisterSystemView<T>();
            }
        }
        IView IForestEngine.RegisterSystemView(Type viewType)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                return ctx.Engine.RegisterSystemView(viewType);
            }
        }
        void ITreeNavigator.Navigate(Location location)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.Navigate(location);
            }
        }
        void ITreeNavigator.NavigateBack()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.NavigateBack();
            }
        }
        void ITreeNavigator.NavigateBack(int offset)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.NavigateBack(offset);
            }
        }
        void ITreeNavigator.NavigateUp()
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.NavigateUp();
            }
        }
        void ITreeNavigator.NavigateUp(int offset)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.NavigateUp(offset);
            }
        }
        
        void ICommandDispatcher.ExecuteCommand(string command, string target, object arg)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.ExecuteCommand(command, target, arg);
            }
        }

        void IMessageDispatcher.SendMessage<T>(T msg)
        {
            using (var ctx = EngineContextProvider.GetContext(this, SystemViewDescriptors, _logger))
            {
                ctx.Engine.SendMessage(msg);
            }
        }
        IView IForestViewFactory.Resolve(IForestViewDescriptor descriptor) => _viewFactory.Resolve(descriptor);
        IView IForestViewFactory.Resolve(IForestViewDescriptor descriptor, object model) => _viewFactory.Resolve(descriptor, model);
        
        IForestViewFactory IForestContext.ViewFactory => this;
        IForestViewRegistry IForestContext.ViewRegistry => _viewRegistry;
        IForestSecurityManager IForestContext.SecurityManager => _securityManager;
        ITemplateProvider IForestContext.TemplateProvider => _templateProvider;
        IForestDomManager IForestContext.DomManager => _domManager;
        IDomProcessor IForestContext.GlobalizationDomProcessor => _forestGlobalizationModule;
        IEnumerable<IForestCommandAdvice> IForestContext.CommandAdvices => _commandAdvices;
        IEnumerable<IForestMessageAdvice> IForestContext.MessageAdvices => _messageAdvices;
        IEnumerable<IForestNavigationAdvice> IForestContext.NavigationAdvices => _navigationAdvices;

        bool IForestCommandAdvice.ExecuteCommand(IExecuteCommandPointcut pointcut)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var result = pointcut.Proceed();
                sw.Stop();
                _logger.Info("Forest ExecuteCommand operation took {0}ms", sw.ElapsedMilliseconds.ToString());
                return result;
            }
            catch (ForestSecurityException securityException)
            {
                _securityExceptionHandler.HandleSecurityException(securityException, pointcut.Command, this);
            }
            return false;
        }

        bool IForestMessageAdvice.SendMessage(ISendMessagePointcut pointcut)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var result = pointcut.Proceed();
                sw.Stop();
                _logger.Info("Forest SendMessage operation took {0}ms", sw.ElapsedMilliseconds.ToString());
                return result;
            }
            catch (ForestSecurityException securityException)
            {
                _securityExceptionHandler.HandleSecurityException(securityException, this);
            }
            return false;
        }

        bool IForestNavigationAdvice.Navigate(INavigatePointcut pointcut)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var result = pointcut.Proceed();
                sw.Stop();
                _logger.Info("Forest Navigate operation took {0}ms", sw.ElapsedMilliseconds.ToString());
                return result;
            }
            catch (ForestSecurityException securityException)
            {
                _securityExceptionHandler.HandleSecurityException(securityException, pointcut.Location, this);
            }
            return false;
        }

        internal IEnumerable<IForestViewDescriptor> SystemViewDescriptors => _viewRegistry.ViewDescriptors.Where(x => x.IsSystemView);
    }
}
