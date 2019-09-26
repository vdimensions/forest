using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;
using Forest.StateManagement;
using Forest.UI;

namespace Forest.Engine
{
    public sealed class MasterExecutionContext : IForestExecutionContext
    {
        private readonly IForestContext _context;
        private readonly IForestStateProvider _stateProvider;
        private readonly PhysicalViewDomProcessor _physicalViewDomProcessor;
        internal IForestExecutionContext _slave;

        internal MasterExecutionContext(IForestContext context, IForestStateProvider stateProvider, PhysicalViewDomProcessor physicalViewDomProcessor)
        {
            _context = context;
            _stateProvider = stateProvider;
            _physicalViewDomProcessor = physicalViewDomProcessor;
        }


        // TODO: Replace this with a constructor of this class that uses a constructor argument, when the class becomes non-abstract (sealed).
        internal IForestExecutionContext CreateActualContext()
        {
            var result = new MasterExecutionContext(_context, _stateProvider, _physicalViewDomProcessor);
            var slaveContext = new SlaveExecutionContext(_context, _stateProvider, _physicalViewDomProcessor, _stateProvider.LoadState(), result);
            result._slave = slaveContext;
            return result;
        }

        private IForestExecutionContext GetActualContext() => _slave ?? CreateActualContext();

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => GetActualContext().ActivateView(instantiateViewInstruction);

        void IDisposable.Dispose() => _slave?.Dispose();

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg) => GetActualContext().ExecuteCommand(command, instanceID, arg);

        IEnumerable<IView> IForestExecutionContext.GetRegionContents(Tree.Node node, string region) => GetActualContext().GetRegionContents(node, region);

        ViewState? IForestExecutionContext.GetViewState(Tree.Node node) => GetActualContext().GetViewState(node);

        void ITreeNavigator.Navigate(string template) => GetActualContext().Navigate(template);

        void ITreeNavigator.Navigate<T>(string template, T message) => GetActualContext().Navigate(template, message);

        void IForestExecutionContext.ProcessInstructions(params ForestInstruction[] instructions) => GetActualContext().ProcessInstructions(instructions);

        T IForestEngine.RegisterSystemView<T>() => GetActualContext().RegisterSystemView<T>();

        void IMessageDispatcher.SendMessage<T>(T message) => GetActualContext().SendMessage(message);

        ViewState IForestExecutionContext.SetViewState(bool silent, Tree.Node node, ViewState viewState) => GetActualContext().SetViewState(silent, node, viewState);

        void IForestExecutionContext.SubscribeEvents(IRuntimeView receiver) => GetActualContext().SubscribeEvents(receiver);

        void IForestExecutionContext.UnsubscribeEvents(IRuntimeView receiver) => GetActualContext().UnsubscribeEvents(receiver);
    }
}