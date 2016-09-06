using System;

using Axle.Forest.UI.Composition;
using Axle.Verification;


namespace Axle.Forest.UI.Messaging
{
    internal class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly IView view;
        private readonly SubscriptionInfo subscription;

        public SubscriptionHandler(IView view, SubscriptionInfo subscription)
        {
            this.view = view.VerifyArgument("view").IsNotNull().Value;
            this.subscription = subscription.VerifyArgument("subscription").IsNotNull().Value;
        }

        public void Invoke(object arg)
        {
            subscription.Invoke(view, arg);
        }

        public Type MessageType { get { return subscription.MessageType; } }
        public IView Receiver { get { return view; } }
    }
}