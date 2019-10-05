using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;

namespace Forest.Engine
{
    internal interface IForestExecutionContext : IForestEngine, IDisposable
    {
        void SubscribeEvents(IRuntimeView receiver);
        void UnsubscribeEvents(IRuntimeView receiver);

        ViewState? GetViewState(Tree.Node node);
        ViewState SetViewState(bool silent, Tree.Node node, ViewState viewState);

        //abstract member GetLinks : id : TreeNode -> List

        void ProcessInstructions(params ForestInstruction[] instructions);

        IView ActivateView(InstantiateViewInstruction instantiateViewInstruction);

        IEnumerable<IView> GetRegionContents(Tree.Node node, string region);

        //abstract member ProcessMessages : unit -> unit
    }
}