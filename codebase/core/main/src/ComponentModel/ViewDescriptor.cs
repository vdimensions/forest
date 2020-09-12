using System;
using System.Collections.Generic;
using System.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class ForestViewDescriptor : IForestViewDescriptor
    {
        public static string GetAnonymousViewName(Type viewType)
        {
            viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>();
            return string.Format("[{1}]::{0}", viewType.FullName, viewType.GetTypeInfo().Assembly.GetName().Name);
        }
        public ForestViewDescriptor(
            string name, 
            Type viewType, 
            Type modelType, 
            IReadOnlyDictionary<string, IForestCommandDescriptor> commands, 
            IEnumerable<IEventDescriptor> events, 
            bool isSystemView, 
            bool isAnonymousView)
        {
            Name = name;
            ViewType = viewType;
            ModelType = modelType;
            Commands = commands;
            Events = events;
            IsSystemView = isSystemView;
            IsAnonymousView = isAnonymousView;
        }

        public string Name { get; }
        public Type ViewType { get; }
        public Type ModelType { get; }
        public IReadOnlyDictionary<string, IForestCommandDescriptor> Commands { get; }
        public IEnumerable<IEventDescriptor> Events { get; }
        public bool IsSystemView { get; }
        public bool IsAnonymousView { get; }
    }
}