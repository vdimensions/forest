using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forest.EventSystem;
using Forest.Reflection;

namespace Forest.Events
{
    public class SubscriptionInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IMethod subscriptionMethod;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Type messageType;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IEnumerable<string> topics;

        public SubscriptionInfo(IMethod subscriptionMethod, params string[] topics)
        {
            if (subscriptionMethod == null)
            {
                throw new ArgumentNullException("subscriptionMethod");
            }
            var parameters = subscriptionMethod.GetParameters();
            if (parameters.Length == 0)
            {
                throw new ArgumentException("Cannot use parameterless method for event subscription.", "subscriptionMethod");
            }
            this.subscriptionMethod = subscriptionMethod;
            this.messageType = parameters.Last().Type;
            this.topics = topics;
        }

        public void Invoke(IView view, object message)
        {
            try
            {
                subscriptionMethod.Invoke(view, message);
            }
            catch (Exception e)
            {
                throw new SubscriptionExecutionException(string.Format("Error executing subscription method {0}", subscriptionMethod.Name), e);
            }
        }

        public Type MessageType { get { return messageType; } }
        public IEnumerable<string> Topics { get { return topics; } }
    }
}