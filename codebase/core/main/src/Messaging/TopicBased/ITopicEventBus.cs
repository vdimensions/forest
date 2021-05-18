using System;

namespace Forest.Messaging.TopicBased
{
    internal interface ITopicEventBus : IEventBus
    {
        /// <summary>
        /// Publishes a message trough the event bus
        /// </summary>
        /// <typeparam name="TMessage">
        /// The type of the message being pushed.
        /// </typeparam>
        /// <param name="sender">
        /// The <see cref="IView">view</see> instance that is sending the message.
        /// </param>
        /// <param name="message">
        /// The object representing the message.
        /// </param>
        /// <param name="topics">
        /// A list of topics names referring to the topics the message will be sent to.
        /// <para>
        /// Use of empty string value <c>""</c> will cause the message to be received by all subscribers for the 
        /// given message type that did not specify topic name during subscription.
        /// </para>
        /// <para>
        /// Use of empty list will cause the message to be broadcast to all subscribers regardless of their subscription topic.
        /// </para>
        /// </param>
        /// <returns>
        /// A <see cref="bool">boolean</see> value indicating whether the message has been sent to a valid topic.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sender"/> is <c>null</c>
        /// </exception>
        void Publish<TMessage>(IView sender, TMessage  message, params string[] topics);
        
        void Subscribe(ISubscriptionHandler subscriptionHandler, string topic);
    }
}