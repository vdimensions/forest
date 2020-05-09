using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;
using Forest.StateManagement;

namespace Forest.Engine.Aspects
{
    internal abstract class AbstractForestExecutionAspect : IForestExecutionAspect, IForestExecutionContext, IStateResolver
    {
        private readonly IForestExecutionContext _chainEC;
        private readonly SlaveExecutionContext _slaveEC;

        protected AbstractForestExecutionAspect(IForestExecutionContext chainEc, SlaveExecutionContext slaveEc)
        {
            _chainEC = chainEc;
            _slaveEC = slaveEc;
        }

        public virtual void SendMessage(IForestExecutionCutPoint cutPoint) => cutPoint.Proceed();
        public virtual void ExecuteCommand(ExecuteCommandCutPoint cutPoint) => cutPoint.Proceed();
        public virtual void Navigate(NavigateCutPoint cutPoint) => cutPoint.Proceed();

        void IMessageDispatcher.SendMessage<T>(T message) => SendMessage(new SendMessageCutPoint<T>(_chainEC, message));

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg) => ExecuteCommand(new ExecuteCommandCutPoint(_chainEC, instanceID, command, arg));

        void ITreeNavigator.Navigate(string template) => Navigate(new NavigateCutPoint(_chainEC, template));
        void ITreeNavigator.Navigate<T>(string template, T message) => Navigate(new NavigateCutPoint(_chainEC, template, message));
        void ITreeNavigator.NavigateBack() => _chainEC.NavigateBack();
        void ITreeNavigator.NavigateUp() => _chainEC.NavigateUp();

        T IForestEngine.RegisterSystemView<T>() => ((IForestExecutionContext) _slaveEC).RegisterSystemView<T>();
        IView IForestEngine.RegisterSystemView(Type viewType) => ((IForestExecutionContext) _slaveEC).RegisterSystemView(viewType);

        public void Dispose() => ((IDisposable) _slaveEC).Dispose();

        void IForestExecutionContext.SubscribeEvents(IRuntimeView receiver) => _slaveEC.SubscribeEvents(receiver);

        void IForestExecutionContext.UnsubscribeEvents(IRuntimeView receiver) => _slaveEC.UnsubscribeEvents(receiver);

        ViewState? IForestExecutionContext.GetViewState(Tree.Node node) => _slaveEC.GetViewState(node);

        ViewState IForestExecutionContext.SetViewState(bool silent, Tree.Node node, ViewState viewState) => _slaveEC.SetViewState(silent, node, viewState);

        void IForestExecutionContext.ProcessInstructions(params ForestInstruction[] instructions) => _slaveEC.ProcessInstructions(instructions);

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => _slaveEC.ActivateView(instantiateViewInstruction);

        IEnumerable<IView> IForestExecutionContext.GetRegionContents(Tree.Node node, string region) => _slaveEC.GetRegionContents(node, region);

        ForestState IStateResolver.ResolveState() => ((IStateResolver) _slaveEC).ResolveState();
    }
}