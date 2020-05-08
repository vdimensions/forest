using System;
using System.Collections.Generic;
using Forest.ComponentModel;
using Forest.StateManagement;
using Forest.UI;

namespace Forest.Engine
{
    public class ForestEngineContextProvider
    {
        private sealed class ForestEngineContext : IForestEngineContext, IForestEngine
        {
            private readonly IForestContext _context;
            private readonly IForestExecutionContext _executionContext;
            private readonly ForestEngineContextProvider _provider;
    
            public ForestEngineContext(IForestContext context, ForestEngineContextProvider provider)
            {
                _context = context;
                _executionContext = new MasterExecutionContext(context, provider.GetForestStateProvider(), provider.GetPhysicalViewRenderer(), this);
                _provider = provider;
            }

            public void Dispose() => _executionContext.Dispose();

            public IForestEngine Engine => _executionContext;
            
            void IMessageDispatcher.SendMessage<T>(T message)
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    ctx.Engine.SendMessage(message);
                }
            }
    
            void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    ctx.Engine.ExecuteCommand(command, instanceID, arg);
                }
            }
    
            void ITreeNavigator.Navigate(string template)
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    ctx.Engine.Navigate(template);
                }
            }
    
            void ITreeNavigator.Navigate<T>(string template, T message)
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    ctx.Engine.Navigate(template, message);
                }
            }
            
            void ITreeNavigator.NavigateBack()
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    ctx.Engine.NavigateBack();
                }
            }
    
            T IForestEngine.RegisterSystemView<T>()
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    return ctx.Engine.RegisterSystemView<T>();
                }
            }
            IView IForestEngine.RegisterSystemView(Type viewType)
            {
                using (var ctx = _provider.CreateContext(_context))
                {
                    return ctx.Engine.RegisterSystemView(viewType);
                }
            }
        }
        
        protected virtual IForestStateProvider GetForestStateProvider() => new DefaultForestStateProvider();
        protected virtual IPhysicalViewRenderer GetPhysicalViewRenderer() => new NoOpPhysicalViewRenderer();

        protected virtual IForestEngineContext CreateContext(IForestContext context) => new ForestEngineContext(context, this);

        internal IForestEngineContext GetContext(IForestContext context, IEnumerable<IViewDescriptor> systemViewDescriptors)
        {
            var engineContext = CreateContext(context);
            foreach (var systemViewDescriptor in systemViewDescriptors)
            {
                engineContext.Engine.RegisterSystemView(systemViewDescriptor.ViewType);
            }
            return engineContext;
        }
    }
}