using System;
using System.Collections.Generic;
using Axle.Verification;
using Forest.Engine.Instructions;

namespace Forest.Engine
{
    public abstract class MasterExecutionContext : IForestExecutionContext
    {
        private readonly IForestExecutionContext _forestContext;

        internal MasterExecutionContext() { }
        internal MasterExecutionContext(IForestExecutionContext forestContext)
        {
            _forestContext = forestContext.VerifyArgument(nameof(forestContext)).IsNotNull().Value;
        }

        // TODO: Implement by instantiating a ForestExecutionContext when that class becomes non-abstract
        internal abstract IForestExecutionContext CreateSlaveContext();

        // TODO: Replace this with a constructor of this class that uses a constructor argument, when the class becomes non-abstract (sealed).
        internal abstract IForestExecutionContext CreateActualContext(IForestExecutionContext slaveContext);

        private IForestExecutionContext GetActualContext() => _forestContext ?? CreateActualContext(CreateSlaveContext());

        IView IForestExecutionContext.ActivateView(InstantiateViewInstruction instantiateViewInstruction) => GetActualContext().ActivateView(instantiateViewInstruction);

        void IDisposable.Dispose() => _forestContext?.Dispose();

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