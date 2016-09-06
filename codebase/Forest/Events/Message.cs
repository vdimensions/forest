using System;
using System.Diagnostics;

namespace Forest.Events
{
    /// <summary>
    /// A class representing a message object sent via the Forest EventBus
    /// </summary>
    [Serializable]
    internal class Message
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object payload;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string topic;

        public Message(object payload, string topic)
        {
            this.payload = payload;
            this.topic = topic;
        }

        public object Payload {  get { return this.payload; } }
        public string Topic {  get { return this.topic; } }
    }
}
