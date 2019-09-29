using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;
using Forest.StateManagement;
using Forest.UI;

namespace Forest.Engine
{
    public sealed class ForestEngine : IForestEngine
    {
        private readonly IForestContext _context;
        private readonly IForestStateProvider _stateProvider;
        private readonly IPhysicalViewRenderer _physicalViewRenderer;

        private void Invoke(Action<IForestEngine> action)
        {
            using (var c = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                action(c);
            }
        }
        private T Invoke<T>(Func<IForestEngine, T> action)
        {
            using (var c = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                return action(c);
            }
        }

        internal ForestEngine(IForestContext context, IForestStateProvider stateProvider, IPhysicalViewRenderer physicalViewRenderer)
        {
            _context = context;
            _stateProvider = stateProvider;
            _physicalViewRenderer = physicalViewRenderer;
        }

        void IMessageDispatcher.SendMessage<T>(T message) => Invoke(x => x.SendMessage(message));

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg) => Invoke(x => x.ExecuteCommand(command, instanceID, arg));

        void ITreeNavigator.Navigate(string template) => Invoke(x => x.Navigate(template));

        void ITreeNavigator.Navigate<T>(string template, T message) => Invoke(x => x.Navigate(template, message));

        T IForestEngine.RegisterSystemView<T>() => Invoke(x => x.RegisterSystemView<T>());
    }

    internal sealed class MasterExecutionContext : IForestExecutionContext
    {
        private readonly IForestContext _context;
        private readonly IForestStateProvider _stateProvider;
        internal IForestExecutionContext _slave;

        internal MasterExecutionContext(IForestContext context, IForestStateProvider stateProvider, IPhysicalViewRenderer physicalViewRenderer, IForestEngine sourceEngine)
        {
            var initialState = stateProvider.LoadState();
            var physicalViewDomProcessor = new PhysicalViewDomProcessor(sourceEngine, physicalViewRenderer, initialState.PhysicalViews);
            var slave = new SlaveExecutionContext(_context = context, physicalViewDomProcessor, initialState, this);
            _slave = slave;
            _stateProvider = stateProvider;
            slave.Init();
        }

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => _slave.ActivateView(instantiateViewInstruction);

        void IDisposable.Dispose()
        {
            try
            {
                _stateProvider.CommitState(((SlaveExecutionContext) _slave).ResolveState());
            }
            catch
            {
                _stateProvider.RollbackState();
                throw;
            }
            finally
            {
                _slave?.Dispose();
            }
        }

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg) => _slave.ExecuteCommand(command, instanceID, arg);

        IEnumerable<IView> IForestExecutionContext.GetRegionContents(Tree.Node node, string region) => _slave.GetRegionContents(node, region);

        ViewState? IForestExecutionContext.GetViewState(Tree.Node node) => _slave.GetViewState(node);

        void ITreeNavigator.Navigate(string template) => _slave.Navigate(template);

        void ITreeNavigator.Navigate<T>(string template, T message) => _slave.Navigate(template, message);

        void IForestExecutionContext.ProcessInstructions(params ForestInstruction[] instructions) => _slave.ProcessInstructions(instructions);

        T IForestEngine.RegisterSystemView<T>() => _slave.RegisterSystemView<T>();

        void IMessageDispatcher.SendMessage<T>(T message) => _slave.SendMessage(message);

        ViewState IForestExecutionContext.SetViewState(bool silent, Tree.Node node, ViewState viewState) => _slave.SetViewState(silent, node, viewState);

        void IForestExecutionContext.SubscribeEvents(IRuntimeView receiver) => _slave.SubscribeEvents(receiver);

        void IForestExecutionContext.UnsubscribeEvents(IRuntimeView receiver) => _slave.UnsubscribeEvents(receiver);
    }
}