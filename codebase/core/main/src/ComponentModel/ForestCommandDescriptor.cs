using System;
using System.Linq;
using Axle.Reflection;
using Axle.Verification;
using Forest.Navigation;

namespace Forest.ComponentModel
{
    internal sealed class ForestCommandDescriptor : AttributeDescriptor<CommandAttribute>, IForestCommandDescriptor
    {
        private readonly IMethod _commandMethod;
        private readonly IParameter _parameter;

        public ForestCommandDescriptor(CommandAttribute attribute, IMethod commandMethod) : base(attribute)
        {
            _parameter = (_commandMethod = commandMethod.VerifyArgument(nameof(commandMethod)).IsNotNull().Value).GetParameters().SingleOrDefault();
        }

        public Location Invoke(IView sender, object arg)
        {
            try
            {
                object navResult;
                if (_parameter != null)
                {
                    navResult = _commandMethod.Invoke(sender, arg ?? _parameter.DefaultValue);
                }
                else
                {
                    navResult = _commandMethod.Invoke(sender) as Location;
                }

                switch (navResult)
                {
                    case string path:
                        return Location.FromPath(path);
                    case Location navigationTarget:
                        return navigationTarget;
                    default:
                        return null;
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