namespace Forest.Messaging.TopicBased
{
    internal interface ITopicEventDescriptor : IEventDescriptor
    {
        string Topic { get; }
    }
}
