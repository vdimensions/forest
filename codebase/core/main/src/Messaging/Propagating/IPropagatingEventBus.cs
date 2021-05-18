namespace Forest.Messaging.Propagating
{
    internal interface IPropagatingEventBus : IEventBus
    {
        void Propagate<TMessage>(IView sender, TMessage message);
        void Subscribe(ISubscriptionHandler subscriptionHandler);
    }
}