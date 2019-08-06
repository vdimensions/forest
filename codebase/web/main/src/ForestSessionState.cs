using System;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    [Serializable]
    internal sealed class ForestSessionState
    {
        private ForestSessionState(State state, object syncRoot)
        {
            State = state;
            Renderer = new DomPhysicalViewRenderer(this);
            SyncRoot = syncRoot;
        }
        internal ForestSessionState(State state) : this(state, new object()) { }
        public ForestSessionState() : this(State.Empty) { }

        public State State { get; }
        internal DomPhysicalViewRenderer Renderer { get; }
        internal object SyncRoot { get; }
    }
}