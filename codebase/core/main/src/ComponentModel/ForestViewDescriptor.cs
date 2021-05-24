using System;
using System.Collections.Generic;
using Axle.Extensions.String;
using Axle.Verification;
using Forest.Commands;
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
            IEnumerable<_TopicEventDescriptor> topicEvents, 
            IEnumerable<_PropagatingEventDescriptor> propagatingEvents, 
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
        public IEnumerable<_TopicEventDescriptor> TopicEvents { get; }
        IEnumerable<ITopicEventDescriptor> IForestViewDescriptor.TopicEvents => TopicEvents;
        public IEnumerable<_PropagatingEventDescriptor> PropagatingEvents { get; }
        IEnumerable<IPropagatingEventDescriptor> IForestViewDescriptor.PropagatingEvents => PropagatingEvents;
        public bool IsSystemView { get; }
        public bool IsAnonymousView { get; }
        public bool TreatNameAsTypeAlias { get; }
    }
}