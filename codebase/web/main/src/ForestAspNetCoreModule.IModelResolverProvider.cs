using System;
using Axle.Web.AspNetCore.Mvc.ModelBinding;
using Forest.Web.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IModelResolverProvider
    {
        IModelResolver IModelResolverProvider.GetModelResolver(Type modelType)
        {
            if (modelType == typeof(IForestMessageArg))
            {
                return new ForestMessageResolver(_messageConverter);
            }
            if (modelType == typeof(IForestCommandArg))
            {
                return new ForestCommandResolver(_sessionStateProvider);
            }
            return null;
        }
        
        void IModelResolverProvider.RegisterTypes(IModelTypeRegistry registry)
        {
            foreach (var viewDescriptor in _viewRegistry.ViewDescriptors)
            {
                registry.Register(viewDescriptor.ModelType);
                foreach (var eventDescriptor in viewDescriptor.Events)
                {
                    if (!string.IsNullOrEmpty(eventDescriptor.Topic))
                    {
                        continue;
                    }
                    registry.Register(eventDescriptor.MessageType);
                    _messageConverter.RegisterMessageType(eventDescriptor.MessageType);
                }
                foreach (var commandDescriptor in viewDescriptor.Commands.Values)
                {
                    if (commandDescriptor.ArgumentType == null)
                    {
                        continue;
                    }
                    registry.Register(commandDescriptor.ArgumentType);
                }
            }
        }
    }
}
