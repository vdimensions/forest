using System;
using System.Linq;
using Axle.Logging;
using Axle.Reflection;
using Axle.Verification;
using Forest.Engine;
using Forest.Navigation;

namespace Forest.ComponentModel
{
    internal sealed class ForestCommandDescriptor : IForestCommandDescriptor
    {
        private sealed class InvocableCommandDescriptor : AttributeDescriptor<CommandAttribute>, IForestCommandDescriptor
        {
            private readonly IMethod _commandMethod;
            private readonly IParameter _parameter;

            public InvocableCommandDescriptor(CommandAttribute attribute, IMethod commandMethod) : base(attribute)
            {
                _parameter = (_commandMethod = commandMethod.VerifyArgument(nameof(commandMethod)).IsNotNull().Value).GetParameters().SingleOrDefault();
            }

            Location IForestCommandDescriptor.Invoke(IForestViewContext context, IView sender, object arg)
            {
                // TODO: use context
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
                            return Location.Create(path);
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

            bool IForestCommandDescriptor.TryResolveRedirect(object arg, out Location redirect)
            {
                redirect = null;
                return false;
            }

            public string Name => Attribute.Name;
            public Type ArgumentType => _parameter?.Type;
            public Location Redirect => null;
        }
        
        private sealed class StaticRedirectCommandDescriptor : AttributeDescriptor<CommandAttribute>, IForestCommandDescriptor
        {
            public StaticRedirectCommandDescriptor(CommandAttribute attribute, Location redirect) : base(attribute)
            {
                Redirect = redirect;
            }

            Location IForestCommandDescriptor.Invoke(IForestViewContext context, IView sender, object arg) => Redirect;
            
            public bool TryResolveRedirect(object _, out Location redirect)
            {
                redirect = Redirect;
                return true;
            }

            public string Name => Attribute.Name;
            public Type ArgumentType => null;
            public Location Redirect { get; }
        }
        
        private sealed class ModelRedirectCommandDescriptor : AttributeDescriptor<CommandAttribute>, IForestCommandDescriptor
        {
            private readonly IMethod _commandMethod;
            private readonly IParameter _parameter;

            public ModelRedirectCommandDescriptor(CommandAttribute attribute, IMethod commandMethod) : base(attribute)
            {
                _parameter = (_commandMethod = commandMethod.VerifyArgument(nameof(commandMethod)).IsNotNull().Value).GetParameters().SingleOrDefault();
            }

            public Location Invoke(IForestViewContext context, IView sender, object arg)
            {
                try
                {
                    object navResult;
                    if (_parameter != null)
                    {
                        navResult = _commandMethod.InvokeStatic(arg ?? _parameter.DefaultValue ?? sender.Model);
                    }
                    else
                    {
                        navResult = _commandMethod.InvokeStatic(sender.Model) as Location;
                    }

                    switch (navResult)
                    {
                        case string path:
                            return Location.Create(path);
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

            bool IForestCommandDescriptor.TryResolveRedirect(object arg, out Location redirect)
            {
                redirect = Invoke(null, null, arg);
                return redirect != null;
            }

            public string Name => Attribute.Name;
            public Type ArgumentType => _parameter?.Type;
        }
        
        public static IForestCommandDescriptor Create(
            Type viewType, 
            Type viewModelType, 
            CommandAttribute attribute, 
            IMethod commandMethod, 
            ILogger logger)
        {
            if (commandMethod.Declaration.IsStatic() && commandMethod.ReturnType == typeof(Location))
            {
                var parameters = commandMethod.GetParameters();
                if (parameters.Length == 0)
                {
                    try
                    {
                        var location = (Location) commandMethod.InvokeStatic();
                        if (location != null)
                        {
                            return new StaticRedirectCommandDescriptor(attribute, location);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(
                            $"Method `{commandMethod.DeclaringType.Name}.{commandMethod.Name}` appears to be a static redirect command but it failed to produce a redirect location due to an error. See inner exception for more details", 
                            e);
                    }
                }
                else if (parameters.Length == 1 && viewModelType.IsAssignableFrom(parameters[0].Type))
                {
                    return new ModelRedirectCommandDescriptor(attribute, commandMethod);
                }
            }
            return new InvocableCommandDescriptor(attribute, commandMethod);
        }
        
        private readonly IForestCommandDescriptor _impl;

        private ForestCommandDescriptor(IForestCommandDescriptor impl)
        {
            _impl = impl;
        }

        public Location Invoke(IForestViewContext context, IView sender, object arg) => _impl.Invoke(context, sender, arg);

        public bool TryResolveRedirect(object arg, out Location redirect) 
            => _impl.TryResolveRedirect(arg, out redirect);

        public string Name => _impl.Name;

        public Type ArgumentType => _impl.ArgumentType;
    }
    
}