using System;
using System.Diagnostics;

namespace Forest.Events
{
    [Serializable]
    public class EventArgs<T> : EventArgs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly T payload;

        public EventArgs() : this(default(T)) { }
        public EventArgs(T payload) { this.payload = payload; }

        public T Payload { get { return payload; } }

        public static implicit operator EventArgs<T>(T payload) { return new EventArgs<T>(payload); }
    }
}