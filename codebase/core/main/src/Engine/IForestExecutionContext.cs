using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;

namespace Forest.Engine
{
    internal interface IForestExecutionContext : IForestEngine, IDisposable
    {
        void SubscribeEvents(_ForestViewContext context, _View receiver);
        void UnsubscribeEvents(_View receiver);

        ViewState GetViewState(string nodeKey);
        ViewState SetViewState(bool silent, string nodeKey, ViewState viewState);
        ViewState UpdateViewState(string nodeKey, Func<ViewState, ViewState> updateFn, bool silent);

        void ProcessInstructions(params ForestInstruction[] instructions);

        IView ActivateView(InstantiateViewInstruction instantiateViewInstruction);

        IEnumerable<IView> GetRegionContents(string nodeKey, string region);
    }
}