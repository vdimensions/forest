using System;
using System.Linq;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.Messaging
{
    internal abstract class AbstractEventDescriptor
    {
        private readonly IParameter _parameter;
        protected readonly IMethod HandlerMethod;
        
        protected AbstractEventDescriptor(IMethod handlerMethod)
        {
            handlerMethod.VerifyArgument(nameof(handlerMethod)).IsNotNull();
            _parameter = (HandlerMethod = handlerMethod).GetParameters().SingleOrDefault();
        }

        protected void DoTrigger(IView sender, object arg)
        {
            if (_parameter != null)
            {
                HandlerMethod.Invoke(sender, arg ?? _parameter.DefaultValue);
            }
            else
            {
                HandlerMethod.Invoke(sender);
            }
        }
        
        public Type MessageType => _parameter?.Type;
    }
}