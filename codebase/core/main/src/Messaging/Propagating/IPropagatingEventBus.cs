namespace Forest.Messaging.Propagating
{
    internal interface IPropagatingEventBus : IEventBus
    {
        void Publish<TMessage>(IView sender, TMessage message, PropagationTargets propagationTargets);
        void Subscribe(ISubscriptionHandler subscriptionHandler);
    }
}