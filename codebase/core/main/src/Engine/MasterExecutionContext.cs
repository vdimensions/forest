using System;
using System.Collections.Generic;
using System.Linq;
using Forest.Engine.Aspects;
using Forest.Engine.Instructions;
using Forest.Navigation;
using Forest.StateManagement;
using Forest.UI;

namespace Forest.Engine
{
    internal sealed class MasterExecutionContext : IForestExecutionContext
    {
        private readonly IForestStateProvider _stateProvider;
        private readonly IForestExecutionContext _slave;

        internal MasterExecutionContext(
            IForestContext context, 
            IForestStateProvider stateProvider, 
            IPhysicalViewRenderer physicalViewRenderer, 
            IForestEngine sourceEngine)
        {
            var initialState = stateProvider.LoadState();
            var physicalViewDomProcessor = new PhysicalViewDomProcessor(sourceEngine, physicalViewRenderer, initialState.PhysicalViews);
            var slave = new SlaveExecutionContext(context, physicalViewDomProcessor, initialState, this);
            _stateProvider = stateProvider;
            var commandAdvices = context.CommandAdvices.Reverse().ToArray();
            var messageAdvices = context.MessageAdvices.Reverse().ToArray();
            var navigationAdvices = context.NavigationAdvices.Reverse().ToArray();
            _slave = commandAdvices.Length > 0 || messageAdvices.Length > 0 || navigationAdvices.Length > 0
                ? new AdvisedForestExecutionContext(slave, commandAdvices, messageAdvices, navigationAdvices)
                : (IForestExecutionContext) slave;
            slave.Init();
        }

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => _slave.ActivateView(instantiateViewInstruction);

        void IDisposable.Dispose()
        {
            try
            {
                _stateProvider.CommitState(((IStateResolver) _slave).ResolveState());
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

        IEnumerable<IView> IForestExecutionContext.GetRegionContents(string nodeKey, string region) => _slave.GetRegionContents(nodeKey, region);

        ViewState? IForestExecutionContext.GetViewState(string nodeKey) => _slave.GetViewState(nodeKey);

        void ITreeNavigator.Navigate(string path) => _slave.Navigate(path);
        void ITreeNavigator.Navigate<T>(string path, T state) => _slave.Navigate(path, state);

        void ITreeNavigator.NavigateBack() => _slave.NavigateBack();
        void ITreeNavigator.NavigateBack(int offset) => _slave.NavigateBack(offset);
        
        void ITreeNavigator.NavigateUp() => _slave.NavigateUp();
        void ITreeNavigator.NavigateUp(int offset) => _slave.NavigateUp(offset);

        void IForestExecutionContext.ProcessInstructions(params ForestInstruction[] instructions) => _slave.ProcessInstructions(instructions);

        T IForestEngine.RegisterSystemView<T>() => _slave.RegisterSystemView<T>();
        IView IForestEngine.RegisterSystemView(Type viewType) => _slave.RegisterSystemView(viewType);

        void IMessageDispatcher.SendMessage<T>(T message) => _slave.SendMessage(message);

        ViewState IForestExecutionContext.SetViewState(bool silent, string nodeKey, ViewState viewState) => _slave.SetViewState(silent, nodeKey, viewState);

        void IForestExecutionContext.SubscribeEvents(IRuntimeView receiver) => _slave.SubscribeEvents(receiver);

        void IForestExecutionContext.UnsubscribeEvents(IRuntimeView receiver) => _slave.UnsubscribeEvents(receiver);
    }
}