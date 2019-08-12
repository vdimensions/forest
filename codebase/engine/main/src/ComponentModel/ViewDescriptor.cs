using System;
using System.Collections.Generic;
using System.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class ViewDescriptor : IViewDescriptor
    {
        public static string GetAnonymousViewName(Type viewType)
        {
            viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>();
            return string.Format("[{1}]::{0}", viewType.FullName, viewType.GetTypeInfo().Assembly.GetName().Name);
        }
        public ViewDescriptor(string name, Type viewType, Type modelType, IReadOnlyDictionary<string, ICommandDescriptor> commands, IReadOnlyDictionary<string, ILinkDescriptor> links, IEnumerable<IEventDescriptor> events, bool isSystemView, bool isAnonymousView)
        {
            Name = name;
            ViewType = viewType;
            ModelType = modelType;
            Commands = commands;
            Events = events;
            Links = links;
            IsSystemView = isSystemView;
            IsAnonymousView = isAnonymousView;
        }

        public string Name { get; }
        public Type ViewType { get; }
        public Type ModelType { get; }
        public IReadOnlyDictionary<string, ILinkDescriptor> Links { get; }
        public IReadOnlyDictionary<string, ICommandDescriptor> Commands { get; }
        public IEnumerable<IEventDescriptor> Events { get; }
        public bool IsSystemView { get; }
        public bool IsAnonymousView { get; }
    }
}