using System;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.Messaging.TopicBased
{
    internal sealed class TopicEventDescriptor : AbstractEventDescriptor, ITopicEventDescriptor
    {
        public TopicEventDescriptor(string topic, IMethod handlerMethod) : base(handlerMethod)
        {
            Topic = topic.VerifyArgument(nameof(topic)).IsNotNull();
        }
        
        public void Trigger(IView sender, object arg)
        {
            try
            {
                DoTrigger(sender, arg);
            }
            catch (Exception e)
            {
                throw new TopicEventInvocationException(sender.GetType(), HandlerMethod, Topic, e);
            }
        }

        public string Topic { get; }
    }
}