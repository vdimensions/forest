using System;
using System.Collections.Generic;
using System.Linq;
using Forest.Engine.Instructions;
using Forest.Navigation;
using Forest.StateManagement;

namespace Forest.Engine.Aspects
{
    internal sealed class AdvisedForestExecutionContext : 
        IForestCommandAdvice, 
        IForestMessageAdvice, 
        IForestNavigationAdvice, 
        IForestExecutionContext, 
        IStateResolver
    {
        private readonly SlaveExecutionContext _executionContext;
        private readonly IEnumerable<IForestCommandAdvice> _commandAdvices;
        private readonly IEnumerable<IForestMessageAdvice> _messageAdvices;
        private readonly IEnumerable<IForestNavigationAdvice> _navigationAdvices;

        public AdvisedForestExecutionContext(
            SlaveExecutionContext executionContext, 
            IEnumerable<IForestCommandAdvice> commandAdvices,
            IEnumerable<IForestMessageAdvice> messageAdvices,
            IEnumerable<IForestNavigationAdvice> navigationAdvices)
        {
            _executionContext = executionContext;
            _commandAdvices = commandAdvices;
            _messageAdvices = messageAdvices;
            _navigationAdvices = navigationAdvices;
        }

        public void ExecuteCommand(IExecuteCommandPointcut pointcut) => pointcut.Proceed();

        public void SendMessage(ISendMessagePointcut pointcut) => pointcut.Proceed();

        public void Navigate(INavigatePointcut pointcut) => pointcut.Proceed();

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg) =>
            ExecuteCommand(_commandAdvices.Aggregate(
                TerminalCommandPointcut.Create(_executionContext, instanceID, command, arg),
                IntermediateCommandPointcut.Create));

        void IMessageDispatcher.SendMessage<T>(T message) =>
            SendMessage(_messageAdvices.Aggregate(
                TerminalMessagePointCut<T>.Create(_executionContext, message),
                IntermediateMessagePointcut.Create));

        void ITreeNavigator.Navigate(string path) =>
            Navigate(_navigationAdvices.Aggregate(
                TerminalNavigatePointcut.Create(_executionContext, path, null),
                IntermediateNavigatePointcut.Create));

        void ITreeNavigator.Navigate<T>(string path, T state) =>
            Navigate(_navigationAdvices.Aggregate(
                TerminalNavigatePointcut.Create(_executionContext, path, state),
                IntermediateNavigatePointcut.Create));

        void ITreeNavigator.NavigateBack() => _executionContext.NavigateBack();
        void ITreeNavigator.NavigateBack(int offset) => _executionContext.NavigateBack(offset);
        
        void ITreeNavigator.NavigateUp() => _executionContext.NavigateUp();
        void ITreeNavigator.NavigateUp(int offset) => _executionContext.NavigateUp(offset);

        T IForestEngine.RegisterSystemView<T>() => ((IForestExecutionContext) _executionContext).RegisterSystemView<T>();
        IView IForestEngine.RegisterSystemView(Type viewType) => ((IForestExecutionContext) _executionContext).RegisterSystemView(viewType);

        public void Dispose() => ((IDisposable) _executionContext).Dispose();

        void IForestExecutionContext.SubscribeEvents(IRuntimeView receiver) => _executionContext.SubscribeEvents(receiver);

        void IForestExecutionContext.UnsubscribeEvents(IRuntimeView receiver) => _executionContext.UnsubscribeEvents(receiver);

        ViewState? IForestExecutionContext.GetViewState(string nodeKey) => _executionContext.GetViewState(nodeKey);

        ViewState IForestExecutionContext.SetViewState(bool silent, string nodeKey, ViewState viewState) => _executionContext.SetViewState(silent, nodeKey, viewState);

        void IForestExecutionContext.ProcessInstructions(params ForestInstruction[] instructions) => _executionContext.ProcessInstructions(instructions);

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => _executionContext.ActivateView(instantiateViewInstruction);

        IEnumerable<IView> IForestExecutionContext.GetRegionContents(string nodeKey, string region) => _executionContext.GetRegionContents(nodeKey, region);

        ForestState IStateResolver.ResolveState() => ((IStateResolver) _executionContext).ResolveState();
    }
}