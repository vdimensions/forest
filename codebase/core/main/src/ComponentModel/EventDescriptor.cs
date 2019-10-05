using System;
using System.Linq;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class EventDescriptor : IEventDescriptor
    {
        private readonly IMethod _handlerMethod;
        private readonly IParameter _parameter;

        public EventDescriptor(string topic, IMethod handlerMethod)
        {
            Topic = topic.VerifyArgument(nameof(topic)).IsNotNull();
            _parameter = (_handlerMethod = handlerMethod.VerifyArgument(nameof(handlerMethod)).IsNotNull().Value).GetParameters().SingleOrDefault();
        }

        public void Trigger(IView sender, object arg)
        {
            try
            {
                if (_parameter != null)
                {
                    _handlerMethod.Invoke(sender, arg ?? _parameter.DefaultValue);
                }
                else
                {
                    _handlerMethod.Invoke(sender);
                }
            }
            catch (Exception e)
            {
                throw new EventInvocationException(sender.GetType(), _handlerMethod, Topic, e);
            }
        }

        public string Topic { get; }
        public Type MessageType => _parameter?.Type;
    }
}