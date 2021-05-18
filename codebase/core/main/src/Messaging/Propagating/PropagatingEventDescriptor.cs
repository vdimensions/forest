using System;
using Axle.Reflection;

namespace Forest.Messaging.Propagating
{
    internal sealed class PropagatingEventDescriptor : AbstractEventDescriptor, IPropagatingEventDescriptor
    {
        public PropagatingEventDescriptor(IMethod handlerMethod) : base(handlerMethod) { }

        public void Trigger(IView sender, object arg)
        {
            try
            {
                DoTrigger(sender, arg);
            }
            catch (Exception e)
            {
                throw new PropagatingEventInvocationException(sender.GetType(), HandlerMethod, e);
            }
        }
    }
}