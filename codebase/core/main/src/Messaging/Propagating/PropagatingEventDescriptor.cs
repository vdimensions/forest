using System;
using Axle.Reflection;
using Forest.Engine;

namespace Forest.Messaging.Propagating
{
    internal sealed class PropagatingEventDescriptor : AbstractEventDescriptor, IPropagatingEventDescriptor
    {
        public PropagatingEventDescriptor(IMethod handlerMethod) : base(handlerMethod) { }

        public void Trigger(_ForestViewContext context, IView sender, object arg)
        {
            try
            {
                DoTrigger(context, sender, arg);
            }
            catch (Exception e)
            {
                throw new PropagatingEventInvocationException(sender.GetType(), HandlerMethod, e);
            }
        }
    }
}