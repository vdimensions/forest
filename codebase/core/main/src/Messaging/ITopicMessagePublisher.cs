namespace Forest.Messaging
{
    public interface ITopicMessagePublisher
    {
        void Publish<T>(T message, params string[] topics);
    }
}