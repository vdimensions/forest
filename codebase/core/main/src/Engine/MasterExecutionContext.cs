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
        private readonly IForestContext _context;

        private bool _discardState;

        internal MasterExecutionContext(
            IForestContext context, 
            IForestStateProvider stateProvider, 
            IPhysicalViewRenderer physicalViewRenderer, 
            IForestEngine sourceEngine)
        {
            _context = context;
            var initialState = stateProvider.BeginUsingState();
            var physicalViewDomProcessor = new PhysicalViewDomProcessor(sourceEngine, physicalViewRenderer, initialState.PhysicalViews);
            var slave = new SlaveExecutionContext(context, physicalViewDomProcessor, initialState, this);
            _stateProvider = stateProvider;
            _slave = slave;
            slave.Init();
        }

        private bool ExecuteCommand(IExecuteCommandPointcut pointcut) => pointcut.Proceed();

        private bool SendMessage(ISendMessagePointcut pointcut) => pointcut.Proceed();

        private bool Navigate(INavigatePointcut pointcut) => pointcut.Proceed();

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => _slave.ActivateView(instantiateViewInstruction);

        void IDisposable.Dispose()
        {
            try
            {
                if (!_discardState)
                {
                    _stateProvider.UpdateState(((IStateResolver) _slave).ResolveState());
                }
            }
            finally
            {
                _stateProvider.EndUsingState();
                _slave?.Dispose();
            }
        }

        IEnumerable<IView> IForestExecutionContext.GetRegionContents(string nodeKey, string region) => _slave.GetRegionContents(nodeKey, region);

        ViewState? IForestExecutionContext.GetViewState(string nodeKey) => _slave.GetViewState(nodeKey);

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
        {
            if (!ExecuteCommand(_context.CommandAdvices.Reverse().Aggregate(
                TerminalCommandPointcut.Create(_slave, instanceID, command, arg),
                IntermediateCommandPointcut.Create)))
            {
                _discardState = true;
            }
        }
        
        void IMessageDispatcher.SendMessage<T>(T message)
        {
            if (!SendMessage(_context.MessageAdvices.Reverse().Aggregate(
                TerminalMessagePointcut<T>.Create(_slave, message),
                IntermediateMessagePointcut.Create)))
            {
                _discardState = true;
            }
        }

        void ITreeNavigator.Navigate(Location location)
        {
            if (!Navigate(_context.NavigationAdvices.Reverse().Aggregate(
                TerminalNavigatePointcut.Create(_slave, location),
                IntermediateNavigatePointcut.Create)))
            {
                _discardState = true;
            }
        }

        void ITreeNavigator.NavigateBack() => _slave.NavigateBack();
        void ITreeNavigator.NavigateBack(int offset) => _slave.NavigateBack(offset);
        
        void ITreeNavigator.NavigateUp() => _slave.NavigateUp();
        void ITreeNavigator.NavigateUp(int offset) => _slave.NavigateUp(offset);

        void IForestExecutionContext.ProcessInstructions(params ForestInstruction[] instructions) => _slave.ProcessInstructions(instructions);

        T IForestEngine.RegisterSystemView<T>() => _slave.RegisterSystemView<T>();
        IView IForestEngine.RegisterSystemView(Type viewType) => _slave.RegisterSystemView(viewType);

        ViewState IForestExecutionContext.SetViewState(bool silent, string nodeKey, ViewState viewState) => _slave.SetViewState(silent, nodeKey, viewState);

        void IForestExecutionContext.SubscribeEvents(IRuntimeView receiver) => _slave.SubscribeEvents(receiver);

        void IForestExecutionContext.UnsubscribeEvents(IRuntimeView receiver) => _slave.UnsubscribeEvents(receiver);
    }
}