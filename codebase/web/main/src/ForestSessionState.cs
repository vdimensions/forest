using System;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    [Serializable]
    internal sealed class ForestSessionState
    {
        private ForestSessionState(ForestState state, object syncRoot)
        {
            State = state;
            Renderer = new DomPhysicalViewRenderer(this);
            SyncRoot = syncRoot;
        }
        internal ForestSessionState(ForestState state) : this(state, new object()) { }
        public ForestSessionState() : this(new ForestState()) { }

        public ForestState State { get; }
        internal DomPhysicalViewRenderer Renderer { get; }
        internal object SyncRoot { get; }
    }
}