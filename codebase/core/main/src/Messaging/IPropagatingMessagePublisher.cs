using Forest.Messaging.Propagating;

namespace Forest.Messaging
{
    public interface IPropagatingMessagePublisher
    {
        void Publish<T>(T message, PropagationTargets targets);
    }
}