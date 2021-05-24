using System;
using Axle.Reflection;
using Axle.Verification;
using Forest.Engine;

namespace Forest.Messaging.TopicBased
{
    internal sealed class TopicEventDescriptor : AbstractEventDescriptor, ITopicEventDescriptor
    {
        public TopicEventDescriptor(string topic, IMethod handlerMethod) : base(handlerMethod)
        {
            Topic = topic.VerifyArgument(nameof(topic)).IsNotNull();
        }
        
        public void Trigger(_ForestViewContext context, IView sender, object arg)
        {
            try
            {
                DoTrigger(context, sender, arg);
            }
            catch (Exception e)
            {
                throw new TopicEventInvocationException(sender.GetType(), HandlerMethod, Topic, e);
            }
        }

        public string Topic { get; }
    }
}