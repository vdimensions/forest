using Forest.Engine;

namespace Forest.Messaging
{
    internal interface _EventDescriptor : IEventDescriptor
    {
        void Trigger(_ForestViewContext context, IView view, object message);
    }
}