using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Forest.Events
{
    /// <summary>
    /// An attribute used to annotate subscription method to an event bus.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class SubscriptionAttribute : Attribute, IEventBusAttribute
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string topic;

        /// <summary>
        /// The communication topic associated with the current subscription.
        /// </summary>
        [DefaultValue("")]
        public string Topic
        {
            get { return topic ?? string.Empty; }
            set { topic = value; }
        }
    }
}