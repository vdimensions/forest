namespace Forest.Messaging
{
    public interface IMessageDispatcher
    {
        void SendMessage<T>(T message);
    }
}
