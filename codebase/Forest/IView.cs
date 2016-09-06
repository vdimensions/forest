namespace Forest
{
    public interface IView
    {
        /// <summary>
        /// Publishes a message to a list of topics.
        /// </summary>
        /// <typeparam name="TMessage">
        /// The type of the object representing the message.
        /// </typeparam>
        /// <param name="message">
        /// The message to be published.
        /// </param>
        /// <param name="topics">
        /// A collection of topic names used to filter the potential subscribers. Leave empty to broadcast to all subscribers.
        /// </param>
        /// <returns>
        /// <c>true</c> if the message was received by at least one subscriber, <c>false</c> otherwise.
        /// </returns>
        bool Publish<TMessage>(TMessage message, params string[] topics);
    }
}