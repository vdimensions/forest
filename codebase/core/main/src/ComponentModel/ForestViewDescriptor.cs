using System;
using System.Collections.Generic;
using Axle.Extensions.String;
using Axle.Verification;
using Forest.Messaging;
using Forest.Messaging.Propagating;
using Forest.Messaging.TopicBased;

namespace Forest.ComponentModel
{
    internal sealed class ForestViewDescriptor : _ForestViewDescriptor
    {
        public static string GetAnonymousViewName(Type viewType)
        {
            viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>();
            return viewType.FullName.TrimEnd("+View", StringComparison.Ordinal);
        }
        public ForestViewDescriptor(
            string name, 
            Type viewType, 
            Type modelType, 
            IReadOnlyDictionary<string, IForestCommandDescriptor> commands, 
            IEnumerable<ITopicEventDescriptor> topicEvents, 
            IEnumerable<IPropagatingEventDescriptor> propagatingEvents, 
            bool isSystemView, 
            bool isAnonymousView,
            bool useNameAsTypeAlias)
        {
            Name = name;
            ViewType = viewType;
            ModelType = modelType;
            Commands = commands;
            TopicEvents = topicEvents;
            PropagatingEvents = propagatingEvents;
            IsSystemView = isSystemView;
            IsAnonymousView = isAnonymousView;
            TreatNameAsTypeAlias = useNameAsTypeAlias;
        }

        public string Name { get; }
        public Type ViewType { get; }
        public Type ModelType { get; }
        public IReadOnlyDictionary<string, IForestCommandDescriptor> Commands { get; }
        public IEnumerable<ITopicEventDescriptor> TopicEvents { get; }
        public IEnumerable<IPropagatingEventDescriptor> PropagatingEvents { get; }
        public bool IsSystemView { get; }
        public bool IsAnonymousView { get; }
        public bool TreatNameAsTypeAlias { get; }
    }
}