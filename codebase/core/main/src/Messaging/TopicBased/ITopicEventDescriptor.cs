namespace Forest.Messaging.TopicBased
{
    public interface ITopicEventDescriptor : IEventDescriptor
    {
        string Topic { get; }
    }
}
