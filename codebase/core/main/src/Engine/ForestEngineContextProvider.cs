using System;
using System.Collections.Generic;
using Axle.Logging;
using Forest.ComponentModel;
using Forest.Messaging;
using Forest.Navigation;
using Forest.StateManagement;
using Forest.UI;

namespace Forest.Engine
{
    public class ForestEngineContextProvider
    {
        private sealed class ForestEngineContext : IForestEngineContext, IForestEngine
        {
            private readonly IForestContext _context;
            private readonly _ForestEngine _engine;
            private readonly ForestEngineContextProvider _provider;
            private readonly ILogger _logger;
    
            public ForestEngineContext(IForestContext context, ForestEngineContextProvider provider, ILogger logger)
            {
                _context = context;
                _engine = new MasterEngine(
                    context, 
                    provider.GetForestStateProvider(), 
                    provider.GetPhysicalViewRenderer(), 
                    this,
                    logger);
                _provider = provider;
                _logger = logger;
            }

            public void Dispose() => _engine.Dispose();

            public IForestEngine Engine => _engine;
            
            void IMessageDispatcher.SendMessage<T>(T message)
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.SendMessage(message);
                }
            }
    
            void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.ExecuteCommand(command, instanceID, arg);
                }
            }
            
            void ITreeNavigator.Navigate(Location location)
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.Navigate(location);
                }
            }
    
            void ITreeNavigator.NavigateBack()
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.NavigateBack();
                }
            }
            void ITreeNavigator.NavigateBack(int offset)
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.NavigateBack(offset);
                }
            }
            
            void ITreeNavigator.NavigateUp()
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.NavigateUp();
                }
            }
            void ITreeNavigator.NavigateUp(int offset)
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    ctx.Engine.NavigateUp(offset);
                }
            }
    
            T IForestEngine.RegisterSystemView<T>()
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    return ctx.Engine.RegisterSystemView<T>();
                }
            }
            IView IForestEngine.RegisterSystemView(Type viewType)
            {
                using (var ctx = _provider.CreateContext(_context, _logger))
                {
                    return ctx.Engine.RegisterSystemView(viewType);
                }
            }
        }
        
        protected virtual IForestStateProvider GetForestStateProvider() => new DefaultForestStateProvider();
        
        protected virtual IPhysicalViewRenderer GetPhysicalViewRenderer() => new NoOpPhysicalViewRenderer();

        protected virtual IForestEngineContext CreateContext(IForestContext context, ILogger logger) => new ForestEngineContext(context, this, logger);

        internal IForestEngineContext GetContext(IForestContext context, IEnumerable<IForestViewDescriptor> systemViewDescriptors, ILogger logger)
        {
            var engineContext = CreateContext(context, logger);
            // TODO: system view instantiation should not happen here
            foreach (var systemViewDescriptor in systemViewDescriptors)
            {
                engineContext.Engine.RegisterSystemView(systemViewDescriptor.ViewType);
            }
            return engineContext;
        }
    }
}