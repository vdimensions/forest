using System;
using System.Collections.Generic;
using Forest.Dom;
using Forest.Engine.Instructions;

namespace Forest.Engine
{
    internal interface IForestExecutionContext : IForestEngine, IDisposable
    {
        void SubscribeEvents(IRuntimeView receiver, Tree.Node node);
        void UnsubscribeEvents(IRuntimeView receiver);

        ViewState? GetViewState(string nodeKey);
        ViewState SetViewState(bool silent, string nodeKey, ViewState viewState);

        void ProcessInstructions(params ForestInstruction[] instructions);

        IView ActivateView(InstantiateViewInstruction instantiateViewInstruction);

        IEnumerable<IView> GetRegionContents(string nodeKey, string region);
    }
}