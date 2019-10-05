using System;
using System.Linq;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class CommandDescriptor : AttributeDescriptor<CommandAttribute>, ICommandDescriptor
    {
        private readonly IMethod _commandMethod;
        private readonly IParameter _parameter;

        public CommandDescriptor(CommandAttribute attribute, IMethod commandMethod) : base(attribute)
        {
            _parameter = (_commandMethod = commandMethod.VerifyArgument(nameof(commandMethod)).IsNotNull().Value).GetParameters().SingleOrDefault();
        }

        public void Invoke(IView sender, object arg)
        {
            try
            {
                if (_parameter != null)
                {
                    _commandMethod.Invoke(sender, arg ?? _parameter.DefaultValue);
                }
                else
                {
                    _commandMethod.Invoke(sender);
                }
            }
            catch (Exception e)
            {
                throw new CommandInvocationException(sender.GetType(), _commandMethod, Name, e);
            }
        }

        public string Name => Attribute.Name;
        public Type ArgumentType => _parameter?.Type;
    }
}